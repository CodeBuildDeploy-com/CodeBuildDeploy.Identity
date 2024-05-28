using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeBuildDeploy.Identity.DA.EF.DI
{
    public static class ServicesRegistration
    {
        private const string EFMigrationsHistoryTableName = "__EFMigrationsHistory";

        public static IServiceCollection ConfigureDataServices(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                options.UseSqlServer(serviceProvider.GetService<IConfiguration>()!.GetConnectionString("AccountConnection"),
                                     x => x.MigrationsHistoryTable(EFMigrationsHistoryTableName, ApplicationDbContext.SchemaName)));

            return services;
        }

        public static IServiceCollection ConfigureDataServices(this IServiceCollection services, string migrationsAssemblyName)
        {
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                options.UseSqlServer(serviceProvider.GetService<IConfiguration>()!.GetConnectionString("AccountMigrationConnection"),
                                     x => x.MigrationsHistoryTable(EFMigrationsHistoryTableName, ApplicationDbContext.SchemaName)
                                           .MigrationsAssembly(migrationsAssemblyName)));

            return services;
        }
    }
}
