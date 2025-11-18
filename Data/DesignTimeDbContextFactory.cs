using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MesEnterprise.Data;
using EFCore.NamingConventions;

namespace MesEnterprise.Data
{
    /// <summary>
    /// This factory is used by the EF Core tools (e.g., for creating migrations) to create a DbContext instance.
    /// It allows design-time operations without needing to run the full application startup.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MesDbContext>
    {
        public MesDbContext CreateDbContext(string[] args)
        {
            // You can use a hardcoded connection string for design-time,
            // or read from a configuration file just for development.
            // This connection string does not need to point to a real database,
            // but it must be syntactically valid for the provider.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=mes_design;Username=postgres;Password=your_dev_password";
            var optionsBuilder = new DbContextOptionsBuilder<MesDbContext>();
            optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

            return new MesDbContext(optionsBuilder.Options);
        }
    }
}