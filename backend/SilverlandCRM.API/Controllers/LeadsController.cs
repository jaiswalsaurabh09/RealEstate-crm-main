using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/leads")]
    public class LeadsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public LeadsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchMobile([FromQuery] string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) return BadRequest(new { message = "Mobile number required" });

            var lead = await _db.Leads
                .Include(l => l.AssignedEmployee)
                .Include(l => l.Project)
                .FirstOrDefaultAsync(l => l.MobileNumber == mobile && !l.IsDeleted);

            var history = await _db.LeadAudits
                .Where(a => a.MobileNumber == mobile)
                .OrderByDescending(a => a.ChangedAt)
                .ToListAsync();

            return Ok(new { lead, auditHistory = history });
        }
    }
}
