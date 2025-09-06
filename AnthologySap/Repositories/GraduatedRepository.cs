using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AnthologySap.Repositories;

public class GraduatedRepository(AppDbContext context)
    : Repository(context: context), IGraduatedRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        string[] statusField = ["GRAD", "COMPLETE"];
        string[] progStatus = ["Titulado", "Egresado"];
        string[] type = ["Doctorado", "Especialidad", "Licenciatura", "Maestria", "Preparatoria"];
        
        return base.Query()
            .Where(e =>
                EF.Constant(type).Contains(e.AsgmtType)
                && EF.Constant(progStatus).Contains(e.ProgStatus)
                && EF.Constant(statusField).Contains(e.StatusField)
            );

    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecGraduatedGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}