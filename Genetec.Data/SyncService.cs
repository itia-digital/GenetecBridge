using Core.Data;
using Core.Data.Extensions;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UP.Data;
using UP.Data.Context;

namespace Genetec.Data;

public class SyncService
{
    private readonly ILogger<SyncService> _logger;
    private readonly SyncWorker _sync = new(new GenetecDbContext());
    private readonly UpDbContext _context = new();

    private readonly ISyncService _students;
    private readonly ISyncService _inactiveStudents;
    private readonly ISyncService _graduated;
    private readonly ISyncService _employees;
    private readonly ISyncService _professors;
    private readonly ISyncService _inactiveEmployees;

    public SyncService(ILogger<SyncService> logger)
    {
        _logger = logger;
        var uow = new UpUnitOfWork(_context);

        _students = new ActiveStudentsSyncService(_sync, uow);
        _inactiveStudents = new InactiveStudentsSyncService(_sync, uow);
        _graduated = new GraduatedSyncService(_sync, uow);
        _employees = new ActiveEmployeesSyncService(_sync, uow);
        _professors = new ActiveProfessorsSyncService(_sync, uow);
        _inactiveEmployees = new InactiveEmployeesSyncService(_sync, uow);
    }

    public async Task ClearAsync()
    {
        await _sync.ResetAsync();
    }

    public async Task SyncAllAsync()
    {
        var datesEnumerator = _context.PsUpIdGralTVws
            .Where(e => e.Lastupddttm != null)
            .Select(e => new { e.Lastupddttm!.Value.Date})
            .Distinct()
            .OrderBy(e => e.Date)
            .FetchAsync();

        await foreach (var datesChunk in datesEnumerator)
        {
            foreach (var d in datesChunk)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                _logger.LogInformation("Syncing {date}..", d.Date);

                await SyncAsync(d.Date);

                watch.Stop();
                _logger.LogInformation(
                    "Syncing {date} finished, elapsed time {elapsed} ms..",
                    d.Date,
                    watch.ElapsedMilliseconds);
            }
        }
    }

    public async Task SyncAsync(DateTime date)
    {
        const int limit = 20000;
        const int chunkSize = 5000;

        // act
        await _students.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date);
        await _inactiveStudents.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date);
        await _graduated.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date);
        await _employees.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date);
        await _professors.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date);
        await _inactiveEmployees.SyncAsync(DateTime.UtcNow, limit, chunkSize, date: date);
    }
}