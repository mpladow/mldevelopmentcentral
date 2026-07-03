using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Infrastructure.Persistence.Configurations;

public sealed class SystemAccountRoleConfiguration : IEntityTypeConfiguration<SystemAccountRole>
{
    public void Configure(EntityTypeBuilder<SystemAccountRole> builder)
    {
        builder.ToTable("SystemAccountRoles");

        builder.HasKey(accountRole => new
        {
            accountRole.SystemDefinitionId,
            accountRole.AccountId,
            accountRole.ApplicationRoleId
        });

        builder.Property(accountRole => accountRole.AccountId)
            .IsRequired();

        builder.HasOne(accountRole => accountRole.SystemDefinition)
            .WithMany(system => system.AccountRoles)
            .HasForeignKey(accountRole => accountRole.SystemDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(accountRole => accountRole.ApplicationRole)
            .WithMany(role => role.AccountRoles)
            .HasForeignKey(accountRole => accountRole.ApplicationRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(accountRole => accountRole.AccountId);
        builder.HasIndex(accountRole => accountRole.ApplicationRoleId);
    }
}
