using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Infrastructure.Persistence.Configurations;

public sealed class ApplicationRolePermissionConfiguration : IEntityTypeConfiguration<ApplicationRolePermission>
{
    public void Configure(EntityTypeBuilder<ApplicationRolePermission> builder)
    {
        builder.ToTable("ApplicationRolePermissions");

        builder.HasKey(rolePermission => new
        {
            rolePermission.ApplicationRoleId,
            rolePermission.ApplicationPermissionId
        });

        builder.HasOne(rolePermission => rolePermission.ApplicationRole)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.ApplicationRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rolePermission => rolePermission.ApplicationPermission)
            .WithMany(permission => permission.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.ApplicationPermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
