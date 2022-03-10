using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TaskManagement.Data
{
    public class ContextSeed
    {
        public static async Task SeedRolesAsync(UserStore<IdentityUser> userStore, RoleStore<IdentityRole> roleStore)
        {
            //Seed Roles
            var role = new IdentityRole(Enums.Roles.Administrator.ToString());
            //role.NormalizedName = Enums.Roles.Administrator.ToString().ToUpper();
            await roleStore.CreateAsync(role);

            role = new IdentityRole(Enums.Roles.User.ToString());
            //role.NormalizedName = Enums.Roles.User.ToString().ToUpper();
            await roleStore.CreateAsync(role);
        }
    }
}
