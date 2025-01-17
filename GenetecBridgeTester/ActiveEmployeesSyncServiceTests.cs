using Genetec.Data;
using Genetec.Data.Context;
using UP.Data;
using UP.Data.Context;
using Xunit.Abstractions;

namespace GenetecBridgeTester;

public class ActiveEmployeesSyncServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly GenetecDbContext _context = new();
    private readonly ActiveEmployeesSyncService _service;

    public ActiveEmployeesSyncServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _service = new ActiveEmployeesSyncService(
            new SyncService(_context),
            new UpUnitOfWork(new UpDbContext())
        );
    }

    [Theory]
    // [InlineData(10, 5)]
    // [InlineData(20, 10)]
    // [InlineData(30, 15)]
    // [InlineData(50, 25)]
    // [InlineData(2000, 1000)]
    [InlineData(10000, 5000)]
    public async Task SyncService_SyncsActiveEmployeesSuccessfully(int limit, int chunkSize)
    {
        // arrange
        await _service.ResetAsync();
        
        // act
        await _service.SyncAsync(limit, chunkSize);
        int createdCount = await _service.CountSyncedAsync();
        _testOutputHelper.WriteLine($"Total records after created: {createdCount.ToString()}");
        
        await _service.SyncAsync(limit, chunkSize);
        int updatedCount = await _service.CountSyncedAsync();
        _testOutputHelper.WriteLine($"Total records after updated: {createdCount.ToString()}");

        // assert
        Assert.True(createdCount > 0);
        Assert.True(updatedCount > 0);
        
        await _service.ResetAsync();
    }
}