using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Infrastructure.Persistence.Configurations;

public sealed class SystemThemeSettingsConfiguration : IEntityTypeConfiguration<SystemThemeSettings>
{
    public void Configure(EntityTypeBuilder<SystemThemeSettings> builder)
    {
        builder.ToTable("SystemThemeSettings");

        builder.HasKey(theme => theme.SystemDefinitionId);

        builder.Property(theme => theme.PrimaryColor)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(theme => theme.SecondaryColor)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(theme => theme.BackgroundColor)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(theme => theme.SurfaceColor)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(theme => theme.TextColor)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(theme => theme.BorderRadius)
            .IsRequired();

        builder.HasOne(theme => theme.SystemDefinition)
            .WithOne(system => system.ThemeSettings)
            .HasForeignKey<SystemThemeSettings>(theme => theme.SystemDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
