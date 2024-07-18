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
using CodeBuildDeploy.Identity.Web.DI;
using CodeBuildDeploy.Identity.Web.EndpointHandlers;
using CodeBuildDeploy.Identity.Web.Middleware;

var logConfiguration = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Async(a => a.Console(new JsonFormatter()));
var reloadableLogger = logConfiguration.CreateBootstrapLogger();
Log.Logger = reloadableLogger;

try
{
    Log.Information("Creating WebApplicationBuilder");
    var builder = WebApplication.CreateBuilder(args);

    Log.Information("Reconfiguring Logging");
    await ConfigureLoggingAsync(builder, reloadableLogger);

    Log.Information("Configuring Services / DI");
    await ConfigureServicesAsync(builder);

    Log.Information("Building WebApplication");
    var app = builder.Build();

    Log.Information("Configuring WebApplication Pipeline Middleware");
    await ConfigureRequestPipelineAsync(app);

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

    builder.Services.ConfigureIdentity(builder.Configuration);

    builder.Services.AddRazorPages();
    builder.Services.AddHealthChecks();

    await Task.CompletedTask;
}

static async Task ConfigureRequestPipelineAsync(WebApplication app)
{
    app.UseForwardedProtoHeader();

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

    app.UseHealthChecks($"/v1/healthcheck");

    app.MapRazorPages();

    var jwtGroup = app.MapGroup("/Identity/jwt");
    jwtGroup.MapGet("gettoken", JWTEndpoint.GetTokenHandler).RequireAuthorization();
    jwtGroup.MapGet("login", JWTEndpoint.GetLoginHandler);
    jwtGroup.MapPost("login", JWTEndpoint.LoginHandler).DisableAntiforgery();

    await Task.CompletedTask;
}