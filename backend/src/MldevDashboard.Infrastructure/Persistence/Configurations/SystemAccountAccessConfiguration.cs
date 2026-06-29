using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MldevDashboard.Domain.Systems;

namespace MldevDashboard.Infrastructure.Persistence.Configurations;

public sealed class SystemAccountAccessConfiguration : IEntityTypeConfiguration<SystemAccountAccess>
{
    public void Configure(EntityTypeBuilder<SystemAccountAccess> builder)
    {
        builder.ToTable("SystemAccountAccesses");

        builder.HasKey(access => new { access.SystemDefinitionId, access.AccountId });

        builder.Property(access => access.AccountId)
            .IsRequired();

        builder.HasOne(access => access.SystemDefinition)
            .WithMany(system => system.AccountAccesses)
            .HasForeignKey(access => access.SystemDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(access => access.AccountId);
    }
}
