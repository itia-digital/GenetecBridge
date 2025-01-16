using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveProfessorsRepository : IRepository<PsUpRhIdDeptdt>;

public class ActiveProfessorsRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpRhIdDeptdt>(context: context),
        IActiveProfessorsRepository
{
    public override IQueryable<PsUpRhIdDeptdt> Query()
    {
        string[] payGroup = ["UPAA001", "UPGA001", "UPMA001", "UPAH001", "UPGH001", "UPMH001"];
        return Table.Where(e => e.HrStatus == "A" 
            && payGroup.Contains(e.GpPaygroup));
    }
}