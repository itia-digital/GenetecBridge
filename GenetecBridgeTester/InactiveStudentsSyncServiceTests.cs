using Core.Data;
using Genetec.Data;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;
using UP.Data;
using UP.Data.Context;

namespace GenetecBridgeTester;

public class InactiveStudentsSyncServiceTests
{
    private readonly GenetecDbContext _context = new();

    private readonly ISyncService _service;
    private readonly SyncWorker _sync;

    public InactiveStudentsSyncServiceTests()
    {
        _sync = new SyncWorker(_context);
        _service = new InactiveStudentsSyncService(_sync,
            new UpUnitOfWork(new UpDbContext()));
    }

    [Theory]
    [InlineData(20000, 5000)]
    public async Task SyncService_SyncsSuccessfully(
        int limit, int chunkSize)
    {
        // arrange
        await _sync.ResetAsync();
        DateTime now = DateTime.Now;

        // act
        await _service.SyncAsync(now, limit, chunkSize);

        // assert
        int result = await _context.AlusaControls.CountAsync();
        Assert.Equal(1, result);
    }
}