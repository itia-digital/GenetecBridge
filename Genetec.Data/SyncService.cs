using Core.Data;
using Core.Data.Extensions;
using Genetec.Data.Context;
using Microsoft.Extensions.Logging;
using UP.Data;
using UP.Data.Context;

namespace Genetec.Data;

public class SyncService : IDisposable, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly UpDbContext _context = new();
    private readonly SyncWorker _sync = new(new GenetecDbContext());

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
        var uow = new UpUnitOfWork(_context);

        _students = new ActiveStudentsSyncService(_sync, uow);
        _graduated = new GraduatedSyncService(_sync, uow);
        _employees = new ActiveEmployeesSyncService(_sync, uow);
        _professors = new ActiveProfessorsSyncService(_sync, uow);
        _inactiveStudents = new InactiveStudentsSyncService(_sync, uow);
        _inactiveEmployees = new InactiveEmployeesSyncService(_sync, uow);
        _inactiveProfessors = new InactiveProfessorsSyncService(_sync, uow);
        _retired = new RetiredSyncService(_sync, uow);
    }

    public async Task ClearAsync()
    {
        await _sync.ResetAsync();
    }

    public async Task SyncAllAsync(DateTime startDate, CancellationToken stoppingToken = default)
    {
        DateTime today = DateTime.Today;
        List<DateTime> dateList = Enumerable.Range(0, (today - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        foreach (var d in dateList)
        {
            await SyncAsync(d.Date, stoppingToken);
        }
    }

    public async Task SyncAsync(DateTime date, CancellationToken stoppingToken)
    {
        const int limit = 20000;
        const int chunkSize = 5000;

        // act
        await _students.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
        await _graduated.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
        await _employees.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
        await _professors.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
        await _inactiveStudents.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
        await _inactiveEmployees.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
        await _inactiveProfessors.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
        await _retired.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date, cancellationToken: stoppingToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}