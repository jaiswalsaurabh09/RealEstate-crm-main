using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Domain.Entities;
using SilverlandCRM.Domain.Enums;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.Infrastructure.Identity;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Email == "admin@silverlandcrm.com"))
            return;

        var admin = new User
        {
            Name = "Silverland Admin",
            Email = "admin@silverlandcrm.com",
            Mobile = "9999999999",
            EmployeeCode = "ADMIN001",
            PasswordHash = PasswordHasher.HashPassword("AdminPass123!"),
            Role = UserRole.Admin,
            IsLoginEnabled = true,
            IsActive = true,
            IsDeleted = false
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
