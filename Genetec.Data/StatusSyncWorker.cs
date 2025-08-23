using Core.Data.Extensions;
using Genetec.Data.Context;
using Genetec.Data.Models;
using Microsoft.EntityFrameworkCore;
using UP.Data;

namespace Genetec.Data;

public class StatusSyncWorker(IUpUnitOfWork up, GenetecDbContext genetec)
{
    /// <summary>
    ///     Syncs all records status (active or inactive).
    /// </summary>
    /// <param name="active"></param>
    /// <param name="limit">Set zero for no limit</param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"></param>
    public async Task<int> SyncAsync(bool active, int limit = 0, int chunkSize = 10000,
        CancellationToken cancellationToken = default)
    {
        var fetchedRecords = up.Status
            .FetchAsync(active, limit, chunkSize, cancellationToken);

        var total = 0;
        await foreach (var source in fetchedRecords)
        {
            var cardHolders = await genetec.Cardholders
                .Where(c => source.Contains(c.UpId!))
                .Select(c => new Cardholder { Guid = c.Guid })
                .ToListAsync(cancellationToken);

            await genetec.ExecUpsertAsync(cardHolders,
                i => $"${i.Guid}",
                i => new { i.Guid },
                (_, value) => new Cardholder
                {
                    Status = active ? (byte)0 : (byte)1
                },
                cancellationToken
            );

            total += source.Count;
        }

        return total;
    }
}