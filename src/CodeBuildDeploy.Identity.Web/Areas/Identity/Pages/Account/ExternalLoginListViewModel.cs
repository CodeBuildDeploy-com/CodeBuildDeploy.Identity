using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace CodeBuildDeploy.Identity.Web.Areas.Identity.Pages.Account
{
    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }

        public string ButtonPrefixText { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}