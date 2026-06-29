using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MldevDashboard.Infrastructure.Persistence;

public sealed class MldevDashboardDbContextFactory
    : IDesignTimeDbContextFactory<MldevDashboardDbContext>
{
    public MldevDashboardDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MldevDashboardDbContext>();

        optionsBuilder.UseSqlServer(
            "server=localhost\\SQLEXPRESS02;Database=local-mldev-db;Trusted_Connection=True;TrustServerCertificate=True");

        return new MldevDashboardDbContext(optionsBuilder.Options);
    }
}
