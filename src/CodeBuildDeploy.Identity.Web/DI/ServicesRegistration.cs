using System.Linq;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeBuildDeploy.Identity.Web.DI
{
    public static class ServicesRegistration
    {
        private const string AuthCookieName = ".AspNet.AuthCookie";

        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
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

            var authBuilder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                                      .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                                      {
                                          options.Cookie.Name = AuthCookieName;
                                          options.Cookie.Path = "/";
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

            return services;
        }
    }
}
