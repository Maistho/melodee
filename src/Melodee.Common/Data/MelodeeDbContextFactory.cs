using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Melodee.Common.Data;

public class MelodeeDbContextFactory : IDesignTimeDbContextFactory<MelodeeDbContext>
{
    public MelodeeDbContext CreateDbContext(string[] args)
    {
        // First try environment variable (for Docker/container scenarios)
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // Fall back to appsettings.json for local development
        if (string.IsNullOrEmpty(connectionString))
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Set the 'ConnectionStrings__DefaultConnection' environment variable " +
                "or provide a connection string in appsettings.json.");
        }

        var builder = new DbContextOptionsBuilder<MelodeeDbContext>();
        builder.UseNpgsql(connectionString, o => o.UseNodaTime().UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        return new MelodeeDbContext(builder.Options);
    }
}
