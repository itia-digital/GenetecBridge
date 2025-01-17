using Core.Data;
using Core.Data.Extensions;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveProfessorsRepository : IRepository;

public class ActiveProfessorsRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpRhIdDeptdt>(context: context),
        IActiveProfessorsRepository
{
    protected override IQueryable<PsUpRhIdDeptdt> Query()
    {
        string[] payGroup = ["UPAA001", "UPGA001", "UPMA001", "UPAH001", "UPGH001", "UPMH001"];
        return Table.Where(e => e.HrStatus == "A" 
            && payGroup.Contains(e.GpPaygroup));
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
                    GenetecGroup = Constants.GenetecInactiveEmployeeGroup
                });
        
        return query.FetchAllRecordsInChunksAsync(chunkSize);
    }
}