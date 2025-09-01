using Core.Data;
using Genetec.Data;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;
using UP.Data;
using UP.Data.Context;

namespace GenetecBridgeTester;

[Collection("Sync")]
public class InactiveEmployeesSyncServiceTests
{
    private readonly GenetecDbContext _context = new();

    private readonly ISyncService _service;

    public InactiveEmployeesSyncServiceTests()
    {
        SyncWorker sync = new(_context, new TestsLogger());
        _service = new InactiveEmployeesSyncService(sync,
            new UpUnitOfWork(new UpDbContext()));
    }

    [Theory]
    [InlineData(20000, 5000)]
    public async Task SyncService_SyncsSuccessfully(
        int limit, int chunkSize)
    {
        // arrange
        //await _sync.ResetAsync();
        DateTime now = DateTime.UtcNow;

        // act
        await _service.SyncAsync(now, limit, chunkSize);

        // assert
        int result = await _context.AlusaControls.CountAsync();
        Assert.True(result > 0);
    }
}