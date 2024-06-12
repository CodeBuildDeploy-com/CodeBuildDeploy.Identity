using System.Linq;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using CodeBuildDeploy.Identity.DA.Entities;
using CodeBuildDeploy.Identity.DA;
using CodeBuildDeploy.Identity.Web.Services;

namespace CodeBuildDeploy.Identity.Web.DI
{
    public static class ServicesRegistration
    {
        private const string AuthCookieName = ".AspNet.AuthCookie";

        public static IServiceCollection ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("CodeBuildDeploy");
            var azureStorageDataProtectionSection = configuration.GetSection("Authentication:DataProtection:AzureStorage");
            if (azureStorageDataProtectionSection.Exists())
            {
                dataProtectionBuilder.PersistKeysToAzureBlobStorage(
                    azureStorageDataProtectionSection["ConnectionString"], 
                    azureStorageDataProtectionSection["ContainerName"], 
                    azureStorageDataProtectionSection["BlobName"]);
            }

            services.ConfigureApplicationCookie(options => {
                options.Cookie.Name = AuthCookieName;
                options.Cookie.Path = "/";
            });

            var authBuilder = services.AddAuthentication(o =>
                                      {
                                          o.DefaultScheme = IdentityConstants.ApplicationScheme;
                                          o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                                      });

            var microsoftAuthSection = configuration.GetSection("Authentication:Microsoft");
            if (microsoftAuthSection.Exists())
            {
                if (microsoftAuthSection.GetChildren().Any(x => x.Key == "ClientId") &&
                    microsoftAuthSection.GetChildren().Any(x => x.Key == "ClientSecret"))
                {
                    authBuilder.AddMicrosoftAccount(microsoftOptions =>
                    {
                        microsoftOptions.ClientId = microsoftAuthSection["ClientId"]!;
                        microsoftOptions.ClientSecret = microsoftAuthSection["ClientSecret"]!;
                        microsoftOptions.CallbackPath = "/Identity/signin-microsoft";
                    });
                }
            }
            var googleAuthSection = configuration.GetSection("Authentication:Google");
            if (googleAuthSection.Exists())
            {
                if (googleAuthSection.GetChildren().Any(x => x.Key == "ClientId") &&
                    googleAuthSection.GetChildren().Any(x => x.Key == "ClientSecret"))
                {
                    authBuilder.AddGoogle(googleOptions =>
                    {
                        googleOptions.ClientId = googleAuthSection["ClientId"]!;
                        googleOptions.ClientSecret = googleAuthSection["ClientSecret"]!;
                        googleOptions.CallbackPath = "/Identity/signin-google";
                    });
                }
            }
            var facebookAuthSection = configuration.GetSection("Authentication:Facebook");
            if (facebookAuthSection.Exists())
            {
                if (facebookAuthSection.GetChildren().Any(x => x.Key == "ClientId") &&
                    facebookAuthSection.GetChildren().Any(x => x.Key == "ClientSecret"))
                {
                    authBuilder.AddFacebook(facebookOptions =>
                    {
                        facebookOptions.ClientId = facebookAuthSection["ClientId"]!;
                        facebookOptions.ClientSecret = facebookAuthSection["ClientSecret"]!;
                        facebookOptions.CallbackPath = "/Identity/signin-facebook";
                    });
                }
            }

            authBuilder.AddIdentityCookies(o => { });

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.SignIn.RequireConfirmedAccount = true;
            }).AddDefaultUI()
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }
    }
}
