using Genetec.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genetec.Services.Configuration;

public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.HasKey(e => e.Guid);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CreationTime)
            .IsRequired();

        builder.Property(e => e.Flags)
            .HasDefaultValue(0);

        builder.Property(e => e.HiddenFromUI)
            .HasComputedColumnSql("CONVERT([bit],[Flags]&(4)) PERSISTED");

        builder.Property(e => e.Federated)
            .HasComputedColumnSql("CONVERT([bit],[Flags]&(8)) PERSISTED");

        builder.HasMany(e => e.Cardholders)
            .WithOne(c => c.Entity)
            .HasForeignKey(c => c.Guid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
