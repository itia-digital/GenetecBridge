using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Genetec.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenetecSyncFunc;

public static class GenetecSyncFunc
{
    [FunctionName("GenetecSyncFunction")]
    public static async Task Run(
        [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
        ILogger log,
        CancellationToken cancellationToken)
    {
        log.LogInformation($"Started at... {DateTime.UtcNow}");

        try
        {
            SyncService syncService = new(log);
            await syncService.SyncAsync(DateTime.UtcNow);
        }
        catch (OperationCanceledException)
        {
            log.LogWarning("Function execution was canceled..");
        }
        catch (Exception ex)
        {
            log.LogError($"Error: {ex.Message}");
        }
    }
}