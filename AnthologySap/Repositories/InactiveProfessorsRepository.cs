using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AnthologySap.Repositories;

public class InactiveProfessorsRepository(AppDbContext context)
    : Repository(context: context), IInactiveProfessorsRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        string[] type = ["Planta", "Honorarios"];
        string[] payGroup =
            ["UPAA001", "UPGA001", "UPMA001", "UPAH001", "UPGH001", "UPMH001"];
        
        return base
            .Query()
            .Where(e =>
                e.StatusField == "I"
                && e.ProgStatus == "Inactivo"
                && EF.Constant(type).Contains(e.AsgmtType)
                && EF.Constant(payGroup).Contains(e.GpPaygroup)
            );
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