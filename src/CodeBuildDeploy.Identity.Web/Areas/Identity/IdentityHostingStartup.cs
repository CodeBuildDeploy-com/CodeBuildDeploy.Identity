using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(CodeBuildDeploy.Areas.Identity.IdentityHostingStartup))]
namespace CodeBuildDeploy.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}