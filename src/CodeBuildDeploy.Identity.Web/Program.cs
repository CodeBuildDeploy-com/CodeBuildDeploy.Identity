using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Formatting.Json;
using Serilog.Extensions.Hosting;

using CodeBuildDeploy.Identity.DA.EF.DI;
using CodeBuildDeploy.Identity.DA.Entities;
using CodeBuildDeploy.Identity.DA;
using Microsoft.AspNetCore.Identity;

var logConfiguration = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Async(a => a.Console(new JsonFormatter()));
var reloadableLogger = logConfiguration.CreateBootstrapLogger();
Log.Logger = reloadableLogger;

try
{
    Log.Information("Creating WebApplicationBuilder");
    var builder = WebApplication.CreateBuilder(args);

    Log.Information("Configuring Configuration");
    await ConfigureConfigurationAsync(builder);

    Log.Information("Reconfiguring Logging");
    await ConfigureLoggingAsync(builder, reloadableLogger);

    Log.Information("Configuring Services");
    await ConfigureServicesAsync(builder);

    Log.Information("Building WebApplication");
    var app = builder.Build();

    Log.Information("Configuring WebApplication");
    await ConfigureAppAsync(app);

    Log.Information("Running WebApplication");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled Exception");
}
finally
{
    Log.Information("WebApplication Shutdown");
    Log.CloseAndFlush();
}

static async Task ConfigureConfigurationAsync(WebApplicationBuilder builder)
{
    // These are already registered
    //var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    //builder.Configuration.AddJsonFile("appsettings.json", false, true);
    //builder.Configuration.AddJsonFile($"appsettings.{env}.json", true, true);
    //builder.Configuration.AddEnvironmentVariables();

    await Task.CompletedTask;
}

static async Task ConfigureLoggingAsync(WebApplicationBuilder builder, ReloadableLogger reloadableLogger)
{
    reloadableLogger.Reload(config => config.ReadFrom.Configuration(builder.Configuration)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Async(a => a.Console(new JsonFormatter())));
    Log.Logger = reloadableLogger.Freeze();

    builder.Host.UseSerilog();
    await Task.CompletedTask;
}

static async Task ConfigureServicesAsync(WebApplicationBuilder builder)
{
    builder.Services.ConfigureDataServices();
    builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ApplicationDbContext>();

    var authBuilder = builder.Services.AddAuthentication();
    var microsoftAuthSection = builder.Configuration.GetSection("Authentication:Microsoft");
    if (microsoftAuthSection != null)
    {
        authBuilder.AddMicrosoftAccount(microsoftOptions =>
        {
            microsoftOptions.ClientId = microsoftAuthSection["ClientId"]!;
            microsoftOptions.ClientSecret = microsoftAuthSection["ClientSecret"]!;
            microsoftOptions.CallbackPath = "/Identity/signin-microsoft";
        });
    }
    var googleAuthSection = builder.Configuration.GetSection("Authentication:Google");
    if (googleAuthSection != null)
    {
        authBuilder.AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = googleAuthSection["ClientId"]!;
            googleOptions.ClientSecret = googleAuthSection["ClientSecret"]!;
            googleOptions.CallbackPath = "/Identity/signin-google";
        });
    }
    var facebookAuthSection = builder.Configuration.GetSection("Authentication:Facebook");
    if (facebookAuthSection != null)
    {
        authBuilder.AddFacebook(facebookOptions =>
        {
            facebookOptions.ClientId = facebookAuthSection["ClientId"]!;
            facebookOptions.ClientSecret = facebookAuthSection["ClientSecret"]!;
            facebookOptions.CallbackPath = "/Identity/signin-facebook";
        });
    }
    builder.Services.AddRazorPages();
    builder.Services.AddHealthChecks();

    await Task.CompletedTask;
}

static async Task ConfigureAppAsync(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();

    app.UseHealthChecks($"/v1/healthcheck");

    await Task.CompletedTask;
}