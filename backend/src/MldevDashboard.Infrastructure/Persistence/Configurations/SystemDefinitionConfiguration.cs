using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Infrastructure.Persistence.Configurations;

public sealed class SystemDefinitionConfiguration : IEntityTypeConfiguration<SystemDefinition>
{
    public void Configure(EntityTypeBuilder<SystemDefinition> builder)
    {
        builder.ToTable("Systems");

        builder.HasKey(system => system.Id);

        builder.Property(system => system.SystemKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(system => system.SystemKey)
            .IsUnique();

        builder.Property(system => system.Label)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(system => system.SortOrder)
            .IsRequired();

        builder.Property(system => system.IsActive)
            .HasDefaultValue(true)
            .IsRequired();
    }
}
