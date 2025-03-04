using Core.Data;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UP.Data;
using UP.Data.Context;

namespace Genetec.Data;

public class SyncService : IDisposable, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly SyncWorker _sync;
    private readonly UpUnitOfWork _uow;
    private readonly UpDbContext _upDb = new();
    private readonly GenetecDbContext _genetecDb = new();

    private readonly ISyncService _students;
    private readonly ISyncService _graduated;
    private readonly ISyncService _employees;
    private readonly ISyncService _professors;
    private readonly ISyncService _inactiveStudents;
    private readonly ISyncService _inactiveEmployees;
    private readonly InactiveProfessorsSyncService _inactiveProfessors;
    private readonly RetiredSyncService _retired;

    public SyncService(ILogger logger)
    {
        _logger = logger;
        _uow = new UpUnitOfWork(_upDb);
        _sync = new SyncWorker(_genetecDb);

        _students = new ActiveStudentsSyncService(_sync, _uow);
        _graduated = new GraduatedSyncService(_sync, _uow);
        _employees = new ActiveEmployeesSyncService(_sync, _uow);
        _professors = new ActiveProfessorsSyncService(_sync, _uow);
        _inactiveStudents = new InactiveStudentsSyncService(_sync, _uow);
        _inactiveEmployees = new InactiveEmployeesSyncService(_sync, _uow);
        _inactiveProfessors = new InactiveProfessorsSyncService(_sync, _uow);
        _retired = new RetiredSyncService(_sync, _uow);
    }

    public async Task ClearAsync()
    {
        await _sync.ResetAsync();
    }

    public async Task SyncSinceAsync(DateTime startDate,
        CancellationToken stoppingToken = default)
    {
        DateTime today = DateTime.Today;
        List<DateTime> dateList = Enumerable.Range(0, (today - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        foreach (var d in dateList) { await SyncAsync(d.Date, stoppingToken); }
    }

    public async Task SyncAsync(DateTime date, CancellationToken stoppingToken)
    {
        const int limit = 20000;
        const int chunkSize = 5000;
        
        await _students.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        await _graduated.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        await _employees.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        await _professors.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        await _inactiveStudents.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        await _inactiveEmployees.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        await _inactiveProfessors.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        await _retired.SyncAsync(DateTime.UtcNow,
            limit,
            chunkSize,
            date: date,
            cancellationToken: stoppingToken);
        
        // Update active-inactive records
        await ActivateAsync(date, stoppingToken);
    }

    private async Task ActivateAsync(DateTime? updatedAt = null,
        CancellationToken cancellationToken = default)
    {
        // 1. fetches from SAPRO
        List<string> activeIds = await _uow.Utilities
            .GetActiveRecordsAsync(updatedAt, cancellationToken);

        if (!activeIds.Any())
        {
            return;
        }

        const string sql = """
                           UPDATE CH
                           SET CH.Status = 0
                           FROM Cardholder CH
                           WHERE UpId IN ($ACTIVE_IDS)
                           """;
        string r = string.Join(',', activeIds.Select(id => $"'{id}'"));

        await _genetecDb.Database.ExecuteSqlRawAsync(sql.Replace("$ACTIVE_IDS", r),
            cancellationToken);
    }

    public void Dispose()
    {
        _upDb.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _upDb.DisposeAsync();
    }
}