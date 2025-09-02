using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public class InactiveProfessorsRepository(AppDbContext context)
    : Repository(context: context), IInactiveProfessorsRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        string[] payGroup =
            ["UPAA001", "UPGA001", "UPMA001", "UPAH001", "UPGH001", "UPMH001"];
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
            Constants.GenetecProfessorGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}