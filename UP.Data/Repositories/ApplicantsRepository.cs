﻿using Core.Data;
using Core.Data.Extensions;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IApplicantsRepositoryRepository : IRepository;

public class ApplicantsRepositoryRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpCsIdProgdt>(context: context),
        IApplicantsRepositoryRepository
{
    protected override IQueryable<PsUpCsIdProgdt> Query()
    {
        return Table.Where(e => e.ProgStatus == "AP");
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAllRecordsInChunksAsync(int chunkSize = 1000)
    {
        IQueryable<UpRecordValue> query = Query()
            .SelectMany(t => Context.PsUpIdGralEVws
                    .Where(e => e.Emplid == t.Emplid)
                    .DefaultIfEmpty(),
                (src, md) => new UpRecordValue
                {
                    Id = src.Emplid,
                    Name = md.FirstName,
                    LastName = md.LastName,
                    Email = md.Emailid,
                    GenetecGroup = Constants.GenetecApplicantGroup
                });
        
        return query.FetchAllRecordsInChunksAsync(chunkSize);
    }
}