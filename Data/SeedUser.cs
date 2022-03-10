using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;

namespace TaskManagement.Data
{
    public class SeedUser
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //Add default roles
            string[] roles = new string[] { "Administrator", "User" };

            foreach (string role in roles)
                if (!context.Roles.Any(r => r.Name == role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            //Add default user
            var userStore = new UserStore<IdentityUser>(context);
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            if (!context.Users.Any(u => u.UserName == "admin@email.com"))
            {
                var adminUser = new IdentityUser("admin@email.com");
                adminUser.NormalizedEmail = adminUser.UserName.ToUpper();
                adminUser.Email = adminUser.UserName;
                adminUser.NormalizedEmail = adminUser.UserName.ToUpper();
                adminUser.EmailConfirmed = true;
                adminUser.LockoutEnabled = true;

                await userManager.CreateAsync(adminUser, "Admin1!");

                //add administrator role
                userManager.AddToRoleAsync(adminUser, "Administrator").Wait();
            }
        }
    }
}
