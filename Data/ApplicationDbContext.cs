using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<TaskManagement.Models.TaskItem> TaskItem { get; set; }
        public DbSet<TaskManagement.Models.Comment> Comment { get; set; }
        public DbSet<TaskManagement.Models.TaskStage> TaskStage { get; set; }
        public DbSet<TaskManagement.Models.FileOnFileSystemModel> FileOnFileSystem { get; set; }
        public DbSet<TaskManagement.Models.Project> Project { get; set; }
        public DbSet<TaskManagement.Models.EmployeeHour> EmployeeHour { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Core Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Core Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

    }
}