using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;

namespace TaskManagement.Models
{
    public class SeedRoles
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            string[] roles = new string[] { "Administrator", "User" };

            foreach (string role in roles)
            {
                var roleStore = new RoleStore<IdentityRole>(context);

                if (!context.Roles.Any(r => r.Name == role))
                {
                    var newRole = new IdentityRole(role);
                    newRole.NormalizedName = role.ToUpper();
                    await roleStore.CreateAsync(newRole);
                }
            }

            var usersStore = new UserStore<IdentityUser>(context);
            if (!context.Users.Any(u => u.UserName == "admin@email.com"))
            {
                var adminUser = new IdentityUser("admin@email.com");
                adminUser.NormalizedEmail = adminUser.UserName.ToUpper();
                adminUser.Email = adminUser.UserName;
                adminUser.NormalizedEmail = adminUser.UserName.ToUpper();
                adminUser.EmailConfirmed = true;
                adminUser.PasswordHash = adminUser.UserName.ToUpper();
                adminUser.LockoutEnabled = true;

                await usersStore.CreateAsync(new IdentityUser("admin@email.com"));
            }
        }
    }
}
