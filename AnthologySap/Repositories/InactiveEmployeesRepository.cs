using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AnthologySap.Repositories;

public class InactiveEmployeesRepository(AppDbContext context)
    : Repository(context: context), IInactiveEmployeesRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base
            .Query()
            .Where(e =>
                e.StatusField == "I"
                && e.ProgStatus == "Inactivo"
                && e.AsgmtType == "Empleado"
            );
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecRetiredGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}