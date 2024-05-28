using CodeBuildDeploy.Identity.DA.EF.DI;

namespace CodeBuildDeploy.Identity.DA.EF.Deploy.DI;

public static class ServicesRegistration
{
    public static IServiceCollection ConfigureApiServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.ConfigureDataServices(typeof(ServicesRegistration).Assembly.FullName!);

        return services;
    }
}
