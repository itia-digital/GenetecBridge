using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AnthologySap.Repositories;

public class InactiveStudentsRepository(AppDbContext context)
    : Repository(context: context),
        IInactiveStudentsRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        string[] type = ["Doctorado", "Especialidad", "Licenciatura", "Maestria", "Preparatoria"];
        return base.Query()
            .Where(e =>
                e.StatusField == "DROP"
                && e.ProgStatus == "Inactivo"
                && EF.Constant(type).Contains(e.AsgmtType)
            );
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecStudentGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}