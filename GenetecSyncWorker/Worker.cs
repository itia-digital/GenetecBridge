using Genetec.Data;
using Microsoft.Data.SqlClient;

namespace GenetecSyncWorker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            logger.LogInformation("Started at: {now}", now);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            logger.LogInformation("Syncing {date}..", now.Date);

            try
            {
                SyncService syncService = new(logger);
                await syncService.SyncAsync(now);
            }
            catch (SqlException e)
            {
                logger.LogError(e, "SQL exception occurred while syncing..");
                if (e.InnerException is not null)
                {
                    logger.LogError(e.InnerException, "Inner exception occurred while syncing..");
                }
                logger.LogError("Syncing {date} failed..", now.Date);
            }
            catch (Exception e)
            {
                logger.LogError(e, "General exception occurred while syncing..");
                if (e.InnerException is not null)
                {
                    logger.LogError(e.InnerException, "Inner exception occurred while syncing..");
                }
                logger.LogError("Syncing {date} failed..", now.Date);
            }
            finally
            {
                watch.Stop();
                logger.LogInformation("Syncing {date} finished, elapsed time {elapsed} ms..", now.Date,
                    watch.ElapsedMilliseconds);
                logger.LogInformation("Finished at: {now}", DateTime.UtcNow);
            }

            // runs every 5m
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}