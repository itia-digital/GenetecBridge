using Core.Data;
using Core.Data.Extensions;
using UP.Data.Context;
using UP.Data.Extension;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IRepository
{
    IAsyncEnumerable<List<UpRecordValue>> FetchAsync(int limit = 0,
        int chunkSize = 1000, CancellationToken cancellationToken = default);
}

public abstract class Repository(UpDbContext context)
{
    protected virtual IQueryable<PsUpIdGralTVw> Query() => context.PsUpIdGralTVws;

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        Guid genetecGroup, int limit = 0, int chunkSize = 1000,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UpRecordValue> query = Query()
            .Select(src => new UpRecordValue
            {
                Id = src.Emplid.Trim(),
                Email = src.Emailid,
                Name = src.FirstName,
                LastName = src.LastName,
                Campus = src.Institution,
                GenetecGroup = genetecGroup,
                PositionOrProgram = src.Descr,
                Type = src.AsgmtType,
                IsActive = src.IsActive(),
            })
            .OrderBy(src => src.Id);

        return query.FetchAsync(limit, chunkSize, cancellationToken);
    }
}