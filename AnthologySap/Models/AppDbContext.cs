using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnthologySap.Models;

public partial class AppDbContext : DbContext
{
    private static ILoggerFactory? _loggerFactory;
    private static LogLevel _minLogLevel = LogLevel.Information;

    // Allows the host to provide the execution logger factory so EF logging
    // is routed through Microsoft.Extensions.Logging (e.g., Serilog sink),
    // not the console.

    // Overload that accepts the minimum Microsoft.Extensions.Logging level
    // so we can turn EF Core logging ON only when running at Debug or lower.
    public static void ConfigureLogging(ILoggerFactory loggerFactory, LogLevel minLevel = LogLevel.Information)
    {
        _loggerFactory = loggerFactory;
        _minLogLevel = minLevel;
        loggerFactory.CreateLogger<AppDbContext>();
    }

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<VUsuariosUnificado> VUsuariosUnificados { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    {
        optionsBuilder
            .UseSqlServer(
                "Server=10.80.0.9;Database=AnthologySync;TrustServerCertificate=True;Integrated Security=True;",
                sqlOptions => sqlOptions.CommandTimeout(60));

        var enableEfLogging = _minLogLevel <= LogLevel.Debug;

        if (enableEfLogging && _loggerFactory != null)
        {
            optionsBuilder.UseLoggerFactory(_loggerFactory);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        // Keep provider caching enabled regardless of log level
        optionsBuilder.EnableServiceProviderCaching();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VUsuariosUnificado>(entity => { entity.ToView("v_UsuariosUnificados"); });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}