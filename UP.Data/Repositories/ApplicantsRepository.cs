using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IApplicantsRepositoryRepository : IRepository<PsUpCsIdProgdt>;

public class ApplicantsRepositoryRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpCsIdProgdt>(context: context),
        IApplicantsRepositoryRepository
{
    public override IQueryable<PsUpCsIdProgdt> Query()
    {
        return Table.Where(e => e.ProgStatus == "AP");
    }
}