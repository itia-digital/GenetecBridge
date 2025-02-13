using System;
using System.Threading.Tasks;
using Genetec.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenetecSyncFunc;

public static class GenetecSyncTimerTrigger
{
    [FunctionName("GenetecSyncTimerTrigger")]
    public static async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
    {
        var now = DateTime.UtcNow;
        log.LogInformation($"Started at: {now}");
        var watch = System.Diagnostics.Stopwatch.StartNew();

        log.LogInformation("Syncing {date}..", now.Date);

        SyncService syncService = new(log);
        await syncService.SyncAsync(now);
        
        watch.Stop();
        log.LogInformation("Syncing {date} finished, elapsed time {elapsed} ms..", now.Date, watch.ElapsedMilliseconds);
        log.LogInformation($"Finished at: {DateTime.UtcNow}");
    }
}