using Microsoft.AspNetCore.Identity;

namespace CodeBuildDeploy.Identity.DA.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual DateTime LastLoginTime { get; set; }
        public virtual DateTime RegistrationDate { get; set; }
    }
}