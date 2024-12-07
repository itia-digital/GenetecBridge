using Genetec.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genetec.Services.Configuration;

public class CardholderConfiguration : IEntityTypeConfiguration<Cardholder>
{
    public void Configure(EntityTypeBuilder<Cardholder> builder)
    {
        builder.HasKey(c => c.Guid);

        builder.Property(c => c.FirstName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.LastName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.MobilePhoneNumber)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.MandatoryEscort)
            .HasDefaultValue(false);

        builder.HasOne(c => c.Entity)
            .WithMany(e => e.Cardholders)
            .HasForeignKey(c => c.Guid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.EscortEntity)
            .WithMany()
            .HasForeignKey(c => c.Escort)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Escort2Entity)
            .WithMany()
            .HasForeignKey(c => c.Escort2)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
