using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ConstructureInfrastructure.Data.Db;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ConstructorDbContext>
{
    public ConstructorDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found.");

        var options = new DbContextOptionsBuilder<ConstructorDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ConstructorDbContext(options);
    }
}
