using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Domain.Entities;

namespace SilverlandCRM.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<LeadAudit> LeadAudits => Set<LeadAudit>();
        public DbSet<EmployeeAudit> EmployeeAudits => Set<EmployeeAudit>();
        public DbSet<ProjectAudit> ProjectAudits => Set<ProjectAudit>();
        public DbSet<ImportHistory> ImportHistories => Set<ImportHistory>();
        public DbSet<IntegrationLog> IntegrationLogs => Set<IntegrationLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b => {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                b.HasIndex(u => u.Mobile);
            });

            modelBuilder.Entity<Lead>(b => {
                b.HasKey(l => l.Id);
                b.HasIndex(l => l.MobileNumber); // Indexed for high performance search
                b.HasIndex(l => l.LeadDate);
                b.HasIndex(l => l.AssignedEmployeeId);
                b.HasIndex(l => l.ProjectId);
                b.HasIndex(l => l.LeadSource);
            });

            modelBuilder.Entity<LeadAudit>(b => {
                b.HasKey(a => a.Id);
                b.HasIndex(a => a.MobileNumber);
                b.HasIndex(a => a.LeadId);
                b.HasIndex(a => a.ChangedAt);
            });
        }
    }
}
