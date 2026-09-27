using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Domain.Entities;
using SilverlandCRM.Infrastructure.Data;
using System.Globalization;
using System.Text;

namespace SilverlandCRM.Infrastructure.Services;

public interface ILeadImportService
{
    Task<ImportSummaryDto> ImportAsync(
        Stream file,
        string fileName,
        Guid userId,
        string userName,
        string portalType = "Generic");
        
    Task<List<ImportHistory>> GetHistoryAsync();
}

public class LeadImportService : ILeadImportService
{
    private readonly ApplicationDbContext _db;

    public LeadImportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ImportSummaryDto> ImportAsync(
        Stream file,
        string fileName,
        Guid userId,
        string userName,
        string portalType = "Generic")
    {
        var batchId = Guid.NewGuid().ToString("N");

        var result = new ImportSummaryDto
        {
            BatchId = batchId,
            FileName = fileName
        };

        portalType = string.IsNullOrWhiteSpace(portalType)
            ? "Generic"
            : portalType.Trim();

        var rows = Path.GetExtension(fileName).Equals(
            ".csv",
            StringComparison.OrdinalIgnoreCase)
            ? await ReadCsvAsync(file)
            : ReadExcel(file);

        var rowNumber = 1;

        foreach (var row in rows)
        {
            rowNumber++;
            result.TotalRows++;

            try
            {
                var name = GetValue(row,
                    "LeadName",
                    "Lead Name",
                    "Name",
                    "Customer Name",
                    "Customer");

                var mobile = NormalizeMobile(GetValue(row,
                    "MobileNumber",
                    "Mobile Number",
                    "Mobile",
                    "Phone",
                    "Phone No.",
                    "Phone Number"));

                var email = GetValue(row,
                    "Email",
                    "Email ID",
                    "Email Address");

                var enquiry = GetValue(row,
                    "EnquiredOn",
                    "Enquired On",
                    "Enquiry",
                    "Enquiry On");

                var sourceLeadId = GetValue(row,
                    "SourceLeadId",
                    "Source Lead Id",
                    "Lead ID",
                    "Lead Id",
                    "Listing ID");

                var sourceListingId = GetValue(row,
                    "SourceListingId",
                    "Source Listing Id",
                    "Listing ID",
                    "Listing Id");

                var response = GetValue(row,
                    "Response",
                    "Status",
                    "Response From");

                var leadSource = portalType;

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(mobile))
                {
                    result.FailedRows++;
                    result.Errors.Add(
                        $"Row {rowNumber}: Lead name and mobile are required.");
                    continue;
                }

                // ----------------------------------------------------
                // Project matching
                // ----------------------------------------------------

                var projectName = GetValue(row,
                    "Project",
                    "Project Name",
                    "ProjectName");

                Guid? projectId = null;

                if (!string.IsNullOrWhiteSpace(projectName))
                {
                    projectId = await _db.Projects
                        .Where(x =>
                            !x.IsDeleted &&
                            x.Name.ToLower() ==
                            projectName.Trim().ToLower())
                        .Select(x => (Guid?)x.Id)
                        .FirstOrDefaultAsync();
                }

                // ----------------------------------------------------
                // Employee matching - Assigned To
                // ----------------------------------------------------

                Guid? assignedEmployeeId = null;

                var assignedTo = GetValue(row,
                    "Assigned To",
                    "AssignedTo",
                    "Assigned Employee",
                    "Employee");

                if (!string.IsNullOrWhiteSpace(assignedTo))
                {
                    assignedEmployeeId = await _db.Users
                        .Where(x =>
                            !x.IsDeleted &&
                            x.Role.ToString() == "Employee" &&
                            x.Name.ToLower() ==
                            assignedTo.Trim().ToLower())
                        .Select(x => (Guid?)x.Id)
                        .FirstOrDefaultAsync();
                }

                // ----------------------------------------------------
                // Existing lead by mobile
                // ----------------------------------------------------

                var existing = await _db.Leads
                    .FirstOrDefaultAsync(x =>
                        x.MobileNumber == mobile &&
                        !x.IsDeleted);

                if (existing != null)
                {
                    existing.LeadName = name;

                    if (!string.IsNullOrWhiteSpace(email))
                        existing.Email = email;

                    if (!string.IsNullOrWhiteSpace(enquiry))
                        existing.EnquiredOn = enquiry;

                    if (projectId.HasValue)
                        existing.ProjectId = projectId;

                    if (assignedEmployeeId.HasValue)
                        existing.AssignedEmployeeId =
                            assignedEmployeeId;

                    if (!string.IsNullOrWhiteSpace(response))
                        existing.Response = response;

                    existing.LeadSource = leadSource;
                    existing.SourceLeadId = sourceLeadId;
                    existing.SourceListingId = sourceListingId;
                    existing.ImportedAt = DateTime.UtcNow;
                    existing.ImportBatchId = batchId;
                    existing.UpdatedAt = DateTime.UtcNow;

                    result.UpdatedRows++;
                    continue;
                }

                var now = DateTime.UtcNow;

                var lead = new Lead
                {
                    LeadDate = GetValue(row,
                        "Date",
                        "LeadDate",
                        "Lead Date"),

                    LeadTime = GetValue(row,
                        "Time",
                        "LeadTime",
                        "Lead Time"),

                    LeadName = name,
                    MobileNumber = mobile,
                    Email = string.IsNullOrWhiteSpace(email)
                        ? null
                        : email,

                    EnquiredOn =
                        string.IsNullOrWhiteSpace(enquiry)
                            ? "Portal Import"
                            : enquiry,

                    Response =
                        string.IsNullOrWhiteSpace(response)
                            ? "New"
                            : response,

                    ProjectId = projectId,

                    LeadSource = leadSource,

                    SourceLeadId = sourceLeadId,
                    SourceListingId = sourceListingId,

                    AssignedEmployeeId =
                        assignedEmployeeId,

                    CreatedByUserId = userId,
                    CreatedAt = now,
                    ImportedAt = now,
                    ImportBatchId = batchId,
                    IsDeleted = false
                };

                if (string.IsNullOrWhiteSpace(lead.LeadDate))
                    lead.LeadDate = now.ToString("yyyy-MM-dd");

                if (string.IsNullOrWhiteSpace(lead.LeadTime))
                    lead.LeadTime = now.ToString("HH:mm:ss");

                _db.Leads.Add(lead);

                result.CreatedRows++;
            }
            catch (Exception ex)
            {
                result.FailedRows++;
                result.Errors.Add(
                    $"Row {rowNumber}: {ex.Message}");
            }
        }

        var history = new ImportHistory
        {
            BatchId = batchId,
            FileName = fileName,
            ImportedByUserName = userName,
            ImportedAt = DateTime.UtcNow,
            TotalRows = result.TotalRows,
            CreatedRows = result.CreatedRows,
            UpdatedRows = result.UpdatedRows,
            DuplicateRows = result.DuplicateRows,
            FailedRows = result.FailedRows,
            Status = result.FailedRows > 0
                ? "CompletedWithErrors"
                : "Completed"
        };

        _db.ImportHistories.Add(history);

        await _db.SaveChangesAsync();

        return result;
    }

    private static string GetValue(
        Dictionary<string, string> row,
        params string[] names)
    {
        foreach (var name in names)
        {
            var key = row.Keys.FirstOrDefault(k =>
                string.Equals(
                    k.Trim(),
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (key != null &&
                row.TryGetValue(key, out var value))
            {
                return value?.Trim() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static string NormalizeMobile(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var mobile = new string(
            value.Where(char.IsDigit).ToArray());

        if (mobile.StartsWith("91") && mobile.Length > 10)
            mobile = mobile[^10..];

        return mobile;
    }

    private static async Task<List<Dictionary<string, string>>> ReadCsvAsync(
        Stream file)
    {
        using var reader = new StreamReader(
            file,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        using var csv = new CsvReader(
            reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            });

        var rows = new List<Dictionary<string, string>>();

        await csv.ReadAsync();
        csv.ReadHeader();

        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        while (await csv.ReadAsync())
        {
            var row = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header))
                    row[header.Trim()] =
                        csv.GetField(header)?.Trim() ?? string.Empty;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static List<Dictionary<string, string>> ReadExcel(
        Stream file)
    {
        var rows = new List<Dictionary<string, string>>();

        using var workbook = new XLWorkbook(file);
        var sheet = workbook.Worksheets.FirstOrDefault();

        if (sheet == null)
            throw new InvalidOperationException(
                "Excel file contains no worksheet.");

        var headerRow = sheet.FirstRowUsed();

        if (headerRow == null)
            return rows;

        var headers = headerRow.CellsUsed()
            .ToDictionary(
                x => x.Address.ColumnNumber,
                x => x.GetString().Trim());

        var firstDataRow = headerRow.RowNumber() + 1;
        var lastDataRow =
            sheet.LastRowUsed()?.RowNumber()
            ?? firstDataRow - 1;

        for (var rowNumber = firstDataRow;
             rowNumber <= lastDataRow;
             rowNumber++)
        {
            var row = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header.Value))
                {
                    row[header.Value] =
                        sheet.Row(rowNumber)
                            .Cell(header.Key)
                            .GetString()
                            .Trim();
                }
            }

            rows.Add(row);
        }

        return rows;
    }

    public async Task<List<ImportHistory>> GetHistoryAsync()
    {
        return await _db.ImportHistories
            .AsNoTracking()
            .OrderByDescending(x => x.ImportedAt)
            .Take(100)
            .ToListAsync();
    }
}
