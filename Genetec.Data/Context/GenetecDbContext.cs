using Genetec.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Context;

public partial class GenetecDbContext : DbContext
{
    public GenetecDbContext()
    {
    }

    public GenetecDbContext(DbContextOptions<GenetecDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cardholder> Cardholders { get; set; }

    public virtual DbSet<CardholderMembership> CardholderMemberships { get; set; }

    public virtual DbSet<CustomFieldValue> CustomFieldValues { get; set; }

    public virtual DbSet<Entity> Entities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=172.25.15.123\\ACCESOS;Database=Directory1;TrustServerCertificate=True;User ID=genetec;Password=genetec");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cardholder>(entity =>
        {
            entity.HasKey(e => e.Guid).IsClustered(false);

            entity.Property(e => e.Guid).ValueGeneratedNever();

            entity.HasOne(d => d.EscortNavigation).WithMany(p => p.CardholderEscortNavigations).HasConstraintName("FK_Cardholder_Escort");

            entity.HasOne(d => d.Escort2Navigation).WithMany(p => p.CardholderEscort2Navigations).HasConstraintName("FK_Cardholder_Escort2");

            entity.HasOne(d => d.Gu).WithOne(p => p.CardholderGu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cardholder_Entity");
        });

        modelBuilder.Entity<CardholderMembership>(entity =>
        {
            entity.HasKey(e => new { e.GuidGroup, e.GuidMember }).IsClustered(false);

            entity.HasOne(d => d.GuidMemberNavigation).WithMany(p => p.CardholderMemberships)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CardholderMembership_Entity");
        });

        modelBuilder.Entity<CustomFieldValue>(entity =>
        {
            entity.HasKey(e => e.Guid).IsClustered(false);

            entity.Property(e => e.Guid).ValueGeneratedNever();
        });

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.Guid).IsClustered(false);

            entity.HasIndex(e => e.CustomType, "IX_EntityCustomType").HasFilter("([CustomType] IS NOT NULL)");

            entity.HasIndex(e => new { e.Name, e.Guid, e.Type }, "IX_EntityName").IsClustered();

            entity.Property(e => e.Guid).ValueGeneratedNever();
            entity.Property(e => e.Federated).HasComputedColumnSql("(CONVERT([bit],[Flags]&(8)))", true);
            entity.Property(e => e.HiddenFromUi).HasComputedColumnSql("(CONVERT([bit],[Flags]&(4)))", true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
