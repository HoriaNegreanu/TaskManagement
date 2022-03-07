using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;

namespace TaskManagement.Models
{
    public class SeedRoles
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            string[] roles = new string[] { "Administrator", "User" };

            foreach (string role in roles)
            {
                var roleStore = new RoleStore<IdentityRole>(context);

                if (!context.Roles.Any(r => r.Name == role))
                {
                    roleStore.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
