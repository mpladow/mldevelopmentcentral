using Microsoft.EntityFrameworkCore;
using MldevDashboard.Domain.Systems;

namespace MldevDashboard.Infrastructure.Persistence;

public sealed class MldevDashboardDbContext(DbContextOptions<MldevDashboardDbContext> options)
    : DbContext(options)
{
    public DbSet<SystemDefinition> Systems => Set<SystemDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MldevDashboardDbContext).Assembly);
    }
}
