using Genetec.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GenetecSyncConsole;

public class Worker(SyncService syncService, ILogger logger) : IDisposable
{
    public async Task SyncAsync(DateTime now, CancellationToken stoppingToken)
    {
        string d = now.ToString("yyyy-MM-dd");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // await syncService.SyncAllAsync(now.AddDays(-60), stoppingToken);
            await syncService.SyncAsync(now, stoppingToken);
        }
        catch (SqlException e)
        {
            logger.LogError(e, "SQL exception occurred while syncing..");
            if (e.InnerException is not null)
            {
                logger.LogError(e.InnerException,
                    "Inner exception occurred while syncing..");
            }

            logger.LogError("Syncing {Date} failed..", d);
        }
        catch (Exception e)
        {
            logger.LogError(e, "General exception occurred while syncing..");
            if (e.InnerException is not null)
            {
                logger.LogError(e.InnerException,
                    "Inner exception occurred while syncing..");
            }

            logger.LogError("Syncing {Date} failed..", d);
        }
        finally
        {
            watch.Stop();
            logger.LogInformation("Syncing {Date} finished, elapsed time {Elapsed} ms..",
                d,
                watch.ElapsedMilliseconds);
            logger.LogInformation("Finished at: {Now}", DateTime.UtcNow);
        }
    }

    public void Dispose()
    {
        syncService.Dispose();
    }
}