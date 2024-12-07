using Microsoft.EntityFrameworkCore;
using UP.Data.Entities;

namespace UP.Data.Context;

public partial class UpDbContext : DbContext
{
    public UpDbContext()
    {
    }

    public UpDbContext(DbContextOptions<UpDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PsUpCsIdProgvw> PsUpCsIdProgvws { get; set; }

    public virtual DbSet<PsUpCsSiUpag> PsUpCsSiUpags { get; set; }

    public virtual DbSet<PsUpCsSiUpgdl> PsUpCsSiUpgdls { get; set; }

    public virtual DbSet<PsUpIdGralVw> PsUpIdGralVws { get; set; }

    public virtual DbSet<PsUpIdInstitVw> PsUpIdInstitVws { get; set; }

    public virtual DbSet<PsUpPersonalMd1> PsUpPersonalMd1s { get; set; }

    public virtual DbSet<PsUpPersonalMod> PsUpPersonalMods { get; set; }

    public virtual DbSet<PsUpRhEmpl> PsUpRhEmpls { get; set; }

    public virtual DbSet<PsUpRhIdDeptvw> PsUpRhIdDeptvws { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=10.80.0.4;Database=SAPRO;TrustServerCertificate=True;Integrated Security=True;");
        // => optionsBuilder.UseSqlServer("Server=10.80.0.4;Database=SAPRO;TrustServerCertificate=True;Integrated Security=True;Connection Timeout=30000");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ACADEMIC\\dproveedoralusa");

        modelBuilder.Entity<PsUpCsIdProgvw>(entity =>
        {
            entity.ToView("PS_UP_CS_ID_PROGVW", "dbo");
        });

        modelBuilder.Entity<PsUpCsSiUpag>(entity =>
        {
            entity.ToView("PS_UP_CS_SI_UPAGS", "dbo");
        });

        modelBuilder.Entity<PsUpCsSiUpgdl>(entity =>
        {
            entity.ToView("PS_UP_CS_SI_UPGDL", "dbo");
        });

        modelBuilder.Entity<PsUpIdGralVw>(entity =>
        {
            entity.ToView("PS_UP_ID_GRAL_VW", "dbo");
        });

        modelBuilder.Entity<PsUpIdInstitVw>(entity =>
        {
            entity.ToView("PS_UP_ID_INSTIT_VW", "dbo");
        });

        modelBuilder.Entity<PsUpPersonalMd1>(entity =>
        {
            entity.ToView("PS_UP_PERSONAL_MD1", "dbo");

            entity.Property(e => e.FirstName).IsFixedLength();
            entity.Property(e => e.LastName).IsFixedLength();
            entity.Property(e => e.SecondLastName).IsFixedLength();
        });

        modelBuilder.Entity<PsUpPersonalMod>(entity =>
        {
            entity.ToView("PS_UP_PERSONAL_MOD", "dbo");

            entity.Property(e => e.FirstName).IsFixedLength();
            entity.Property(e => e.LastName).IsFixedLength();
            entity.Property(e => e.SecondLastName).IsFixedLength();
        });

        modelBuilder.Entity<PsUpRhEmpl>(entity =>
        {
            entity.ToView("PS_UP_RH_EMPLS", "dbo");
        });

        modelBuilder.Entity<PsUpRhIdDeptvw>(entity =>
        {
            entity.ToView("PS_UP_RH_ID_DEPTVW", "dbo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
