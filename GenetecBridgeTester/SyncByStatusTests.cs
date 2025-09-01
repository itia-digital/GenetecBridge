using Core.Data;
using Genetec.Data;
using Genetec.Data.Context;
using UP.Data;
using UP.Data.Context;

namespace GenetecBridgeTester;

public class SyncByStatusTests
{
    private readonly StatusSyncService _syncService = new(
        new UpUnitOfWork(new UpDbContext()),
        new GenetecDbContext(),
        Utils.GetLogger<StatusSyncService>()
    );

    [Fact]
    public async Task SyncService_SyncsAllSuccessfully()
    {
        await _syncService.SyncAsync(CancellationToken.None);
    }
}