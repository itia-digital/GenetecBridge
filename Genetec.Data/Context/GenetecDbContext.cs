using Core.Data.Extensions;
using Genetec.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Context;

public class GenetecDbContext(DbContextOptions<GenetecDbContext> options)
    : DbContext(options)
{
    public DbSet<Entity> Entities { get; set; }
    public DbSet<Cardholder> Cardholders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations dynamically
        modelBuilder.ApplyAllConfigurations(typeof(GenetecDbContext).Assembly);
    }
}