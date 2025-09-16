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
        return base
            .Query()
            .Where(e =>
                e.StatusField == "I"
                && e.ProgStatus == "Inactivo"
                && e.AsgmtType == "Profesor"
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