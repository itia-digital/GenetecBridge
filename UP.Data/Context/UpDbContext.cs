using Microsoft.EntityFrameworkCore;
using UP.Data.Models;

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

    public virtual DbSet<PsUpIdGralTVw> PsUpIdGralTVws { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=10.80.0.4;Database=SAPRO;TrustServerCertificate=True;User ID=ControlAccesos;Password=C0n7r0lA<<esos");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ACADEMIC\\dproveedoralusa");

        modelBuilder.Entity<PsUpIdGralTVw>(entity =>
        {
            entity.ToView("PS_UP_ID_GRAL_T_VW", "dbo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
