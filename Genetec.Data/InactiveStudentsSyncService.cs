﻿using Core.Data;
using UP.Data;

namespace Genetec.Data;

public class InactiveStudentsSyncService(SyncWorker worker, IUpUnitOfWork unitOfWork) : ISyncService
{
    /// <summary>
    ///     Syncs records and set the same genetec access group
    ///     to all the records
    /// </summary>
    /// <param name="startedAt"></param>
    /// <param name="limit">Set zero for no limit</param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"></param>
    public async Task SyncAsync(DateTime startedAt, int limit = 0, int chunkSize = 2000,
        CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<List<UpRecordValue>> fetchedRecords = unitOfWork.InactiveStudents
            .FetchAsync(limit, chunkSize, cancellationToken);

        await foreach (List<UpRecordValue> upItems in fetchedRecords)
            await worker.RunAsync(startedAt, upItems, cancellationToken);
    }
}