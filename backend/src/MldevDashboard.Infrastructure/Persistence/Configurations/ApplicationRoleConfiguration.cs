using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Infrastructure.Persistence.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRoles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.Property(role => role.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(role => role.IsProtected)
            .IsRequired();

        builder.Property(role => role.SortOrder)
            .IsRequired();

        builder.HasOne(role => role.SystemDefinition)
            .WithMany(system => system.Roles)
            .HasForeignKey(role => role.SystemDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
