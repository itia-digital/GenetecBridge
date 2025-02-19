using System;
using System.Threading.Tasks;
using Genetec.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenetecSyncFunc;

public static class GenetecSyncTimerTrigger
{
    /// <summary>
    /// '0 0 6-22 * * 1-6' Each hr  from 6AM to 10 PM, except on sunday
    /// '0 0 6-22 * * *' Each hr  from 6AM to 10 PM
    /// '0 */5 * * * *' Every 2m
    /// </summary>
    /// <param name="myTimer"></param>
    /// <param name="log"></param>
    [FunctionName("GenetecSyncTimerTrigger")]
    public static async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
    {
        var now = DateTime.UtcNow;
        log.LogInformation($"Started at: {now}");
        var watch = System.Diagnostics.Stopwatch.StartNew();

        log.LogInformation("Syncing {date}..", now.Date);

        try
        {
            SyncService syncService = new(log);
            await syncService.SyncAsync(now);
        }
        catch (Exception e)
        {
            log.LogError((e.InnerException ?? e).Message);
            log.LogError((e.InnerException ?? e).StackTrace);
            log.LogError("Syncing {date} failed..", now.Date);
            throw;
        }
        finally
        {
            watch.Stop();
            log.LogInformation("Syncing {date} finished, elapsed time {elapsed} ms..", now.Date,
                watch.ElapsedMilliseconds);
            log.LogInformation($"Finished at: {DateTime.UtcNow}");
        }
    }
}