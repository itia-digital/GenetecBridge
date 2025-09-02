using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;

namespace AnthologySap.Repositories;

public class GraduatedRepository(AppDbContext context)
    : Repository(context: context), IGraduatedRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base
            .Query()
            .Where(e =>
                (e.StatusField == "DM" && e.ProgStatus == "Egresado")
                || (e.StatusField == "CM" && e.ProgStatus == "CRED")
                || (e.StatusField == "SP" && e.ProgStatus == "Egresado")
                || (e.StatusField == "SP" && e.ProgStatus == "EGRP")
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