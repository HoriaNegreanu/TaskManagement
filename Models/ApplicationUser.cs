using Microsoft.AspNetCore.Identity;

namespace TaskManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ApplicationUser() : base()
        {
        }

        public ApplicationUser(string userName)
            : base(userName)
        {
        }
    }
}
