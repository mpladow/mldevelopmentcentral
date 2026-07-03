using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Infrastructure.Persistence.Configurations;

public sealed class ApplicationPermissionConfiguration : IEntityTypeConfiguration<ApplicationPermission>
{
    public void Configure(EntityTypeBuilder<ApplicationPermission> builder)
    {
        builder.ToTable("ApplicationPermissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.PermissionKey)
            .HasMaxLength(160)
            .IsRequired();

        builder.HasIndex(permission => permission.PermissionKey)
            .IsUnique();

        builder.Property(permission => permission.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(permission => permission.Category)
            .HasMaxLength(96)
            .IsRequired();

        builder.HasOne(permission => permission.SystemDefinition)
            .WithMany(system => system.Permissions)
            .HasForeignKey(permission => permission.SystemDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
