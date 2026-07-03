using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Infrastructure.Persistence;

public sealed class MldevDashboardDbContext(DbContextOptions<MldevDashboardDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<SystemDefinition> Systems => Set<SystemDefinition>();

    public DbSet<SystemAccountAccess> SystemAccountAccesses => Set<SystemAccountAccess>();

    public DbSet<SystemThemeSettings> SystemThemeSettings => Set<SystemThemeSettings>();

    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();

    public DbSet<ApplicationPermission> ApplicationPermissions => Set<ApplicationPermission>();

    public DbSet<ApplicationRolePermission> ApplicationRolePermissions => Set<ApplicationRolePermission>();

    public DbSet<SystemAccountRole> SystemAccountRoles => Set<SystemAccountRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MldevDashboardDbContext).Assembly);
    }
}
