using Microsoft.AspNetCore.Identity;

namespace CodeBuildDeploy.Identity.DA.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [ProtectedPersonalData]
        public string FirstName { get; set; }

        [ProtectedPersonalData]
        public string LastName { get; set; }

        [PersonalData]
        public virtual DateTime LastLoginTime { get; set; }

        [PersonalData]
        public virtual DateTime RegistrationDate { get; set; }
    }
}