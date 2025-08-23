﻿using Core.Data;
using Core.Data.Extensions;
using UP.Data.Context;
using UP.Data.Extension;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IRepository
{
    IAsyncEnumerable<List<UpRecordValue>> FetchAsync(int limit = 0, int chunkSize = 1000,
        DateTime? date = null, CancellationToken cancellationToken = default);
}

public abstract class Repository(UpDbContext context)
{
    protected virtual IQueryable<PsUpIdGralTVw> Query() => context.PsUpIdGralTVws.Select(
        e => new PsUpIdGralTVw
        {
            Emplid = e.Emplid,
            FirstName = e.FirstName,
            LastName = e.LastName,
            SecondLastName = e.SecondLastName,
            Sex = e.Sex,
            Institution = e.Institution,
            Emailid = e.Emailid,
            AcadProg = e.AcadProg,
            Descr = e.Descr,
            GpPaygroup = e.GpPaygroup,
            StatusField = e.StatusField,
            ProgReason = e.ProgReason,
            Lastupddttm = e.Lastupddttm,
            AsgmtType = e.AsgmtType
        });

    protected IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        Guid genetecGroup, int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UpRecordValue> query = Query()
            .ConditionalWhere(date != null,
                e => e.Lastupddttm!.Value.Date == date!.Value.Date)
            .Select(src => new UpRecordValue
            {
                Id = src.Emplid.Trim(),
                Email = src.Emailid,
                Name = src.FirstName.Trim(),
                LastName = $"{src.LastName.Trim()} {src.SecondLastName.Trim()}",
                Campus = src.Institution.Trim(),
                GenetecGroup = genetecGroup,
                PositionOrProgram = src.Descr.Trim(),
                Type = src.AsgmtType,
                IsActive = src.IsActive(),
            })
            .OrderBy(src => src.Id);

        return query.FetchAsync(limit, chunkSize, cancellationToken);
    }
}