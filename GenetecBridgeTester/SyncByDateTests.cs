using Core.Data;
using Genetec.Data;

namespace GenetecBridgeTester;

public class SyncByDateTests
{
    private readonly SyncService _syncService = new(Utils.GetLogger<SyncService>());

    [Fact]
    public async Task SyncService_SyncsAllSuccessfully()
    {
        // arrange
        await _syncService.ClearAsync();
        
        // act
        await _syncService.SyncAllAsync(DateTime.Today, CancellationToken.None);

        // assert
    }

    [Theory]
    //[InlineData("2025-02-14")]
    //[InlineData("2025-02-13")]
    [InlineData("2016-06-06")]
    [InlineData("2024-06-06")]
    //[InlineData("2002-05-29")]
    public async Task SyncService_SyncsSuccessfully(
        string date)
    {
        // arrange
        // act
        await _syncService.SyncAsync(DateTime.Parse(date), CancellationToken.None);

        // assert;
    }
}