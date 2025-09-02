﻿using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public class InactiveEmployeesRepository(AppDbContext context)
    : Repository(context: context), IInactiveEmployeesRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        string[] payGroup = ["UPA001", "UPC001", "UPE001", "UPG001", "UPM001"];
        return base
            .Query()
            .Where(e => e.StatusField == "I"
                        && EF.Constant(payGroup).Contains(e.GpPaygroup));
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecRetiredGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}