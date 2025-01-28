using Genetec.Data;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;
using UP.Data;
using UP.Data.Context;

namespace GenetecBridgeTester;

public class ActiveProfessorsSyncServiceTests
{
    private readonly GenetecDbContext _context = new();

    private readonly ActiveProfessorsSyncService _service;
    private readonly SyncWorker _sync;

    public ActiveProfessorsSyncServiceTests()
    {
        _sync = new SyncWorker(_context);
        _service = new ActiveProfessorsSyncService(_sync,
            new UpUnitOfWork(new UpDbContext()));
    }

    [Theory]
    [InlineData(20000, 5000)]
    public async Task SyncService_SyncsSuccessfully(int limit,
        int chunkSize)
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