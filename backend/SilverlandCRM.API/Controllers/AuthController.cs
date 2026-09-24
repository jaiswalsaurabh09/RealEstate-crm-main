using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Infrastructure.Data;
using SilverlandCRM.Infrastructure.Identity;

namespace SilverlandCRM.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtTokenGenerator _jwt;

        public AuthController(ApplicationDbContext db, JwtTokenGenerator jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            if (!user.IsLoginEnabled)
            {
                return StatusCode(403, new { message = "User account disabled by Administrator." });
            }

            var token = _jwt.GenerateToken(user);
            return Ok(new { token, user = new { user.Id, user.Name, user.Email, Role = user.Role.ToString() } });
        }
    }

    public record LoginRequest(string Email, string Password);
}
