using Genetec.Data;
using Genetec.Data.Context;
using UP.Data;
using UP.Data.Context;

namespace GenetecBridgeTester;

public class ActiveEmployeesSyncServiceTests
{
    private readonly ActiveEmployeesSyncService _service =
        new(
            new SyncService(new GenetecDbContext()),
            new UpUnitOfWork(new UpDbContext())
        );

    [Fact]
    public async Task SyncService_SyncsActiveEmployeesSuccessfully()
    {
        await _service.SyncAsync(10, 5);
    }
}