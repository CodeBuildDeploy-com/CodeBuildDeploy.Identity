using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using CodeBuildDeploy.Identity.DA.Entities;

namespace CodeBuildDeploy.Identity.DA
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public const string SchemaName = "idnt";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            base.OnModelCreating(modelBuilder);
        }
    }
}