using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Domain.Entities;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.Infrastructure.Services;

public interface ILeadImportService
{
    Task<ImportSummaryDto> ImportAsync(
        Stream file,
        string fileName,
        Guid userId,
        string userName);

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
        string userName)
    {
        var batchId = Guid.NewGuid().ToString("N");

        var result = new ImportSummaryDto
        {
            BatchId = batchId,
            FileName = fileName
        };

        using var workbook = new XLWorkbook(file);
        var sheet = workbook.Worksheets.FirstOrDefault();

        if (sheet == null)
            throw new InvalidOperationException("Excel file contains no worksheet.");

        var headerMap = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        var headerRow = sheet.FirstRowUsed();

        foreach (var cell in headerRow.CellsUsed())
        {
            var name = cell.GetString().Trim();

            if (!string.IsNullOrWhiteSpace(name))
                headerMap[name] = cell.Address.ColumnNumber;
        }

        string GetValue(IXLRow row, params string[] names)
        {
            foreach (var name in names)
            {
                if (headerMap.TryGetValue(name, out var col))
                    return row.Cell(col).GetString().Trim();
            }

            return string.Empty;
        }

        var firstDataRow = headerRow.RowNumber() + 1;
        var lastDataRow = sheet.LastRowUsed()?.RowNumber() ?? firstDataRow - 1;

        for (var rowNumber = firstDataRow;
             rowNumber <= lastDataRow;
             rowNumber++)
        {
            result.TotalRows++;

            try
            {
                var row = sheet.Row(rowNumber);

                var name = GetValue(
                    row,
                    "LeadName",
                    "Lead Name",
                    "Name");

                var mobile = GetValue(
                    row,
                    "MobileNumber",
                    "Mobile Number",
                    "Mobile",
                    "Phone");

                var email = GetValue(
                    row,
                    "Email");

                var enquiredOn = GetValue(
                    row,
                    "EnquiredOn",
                    "Enquired On",
                    "Enquiry",
                    "Enquiry On");

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(mobile))
                {
                    result.FailedRows++;
                    result.Errors.Add(
                        $"Row {rowNumber}: Lead name and mobile are required.");
                    continue;
                }

                var existing = await _db.Leads
                    .FirstOrDefaultAsync(x =>
                        x.MobileNumber == mobile &&
                        !x.IsDeleted);

                if (existing != null)
                {
                    existing.LeadName = name;
                    existing.Email = string.IsNullOrWhiteSpace(email)
                        ? existing.Email
                        : email;
                    existing.EnquiredOn = string.IsNullOrWhiteSpace(enquiredOn)
                        ? existing.EnquiredOn
                        : enquiredOn;
                    existing.ImportedAt = DateTime.UtcNow;
                    existing.ImportBatchId = batchId;
                    existing.UpdatedAt = DateTime.UtcNow;

                    result.UpdatedRows++;
                    continue;
                }

                var now = DateTime.UtcNow;

                var lead = new Lead
                {
                    LeadDate = now.ToString("yyyy-MM-dd"),
                    LeadTime = now.ToString("HH:mm:ss"),
                    LeadName = name,
                    MobileNumber = mobile,
                    Email = email,
                    EnquiredOn = string.IsNullOrWhiteSpace(enquiredOn)
                        ? "Excel Import"
                        : enquiredOn,
                    Response = GetValue(row, "Response", "Status"),
                    LeadSource = GetValue(
                        row,
                        "LeadSource",
                        "Lead Source",
                        "Source"),

                    SourceLeadId = GetValue(
                        row,
                        "SourceLeadId",
                        "Source Lead Id"),

                    SourceListingId = GetValue(
                        row,
                        "SourceListingId",
                        "Source Listing Id"),

                    CreatedByUserId = userId,
                    CreatedAt = now,
                    ImportedAt = now,
                    ImportBatchId = batchId,
                    IsDeleted = false
                };

                if (string.IsNullOrWhiteSpace(lead.Response))
                    lead.Response = "New";

                if (string.IsNullOrWhiteSpace(lead.LeadSource))
                    lead.LeadSource = "Excel";

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

    public async Task<List<ImportHistory>> GetHistoryAsync()
    {
        return await _db.ImportHistories
            .AsNoTracking()
            .OrderByDescending(x => x.ImportedAt)
            .Take(100)
            .ToListAsync();
    }
}
