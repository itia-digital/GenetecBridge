using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using AnthologySap.Extension;

namespace AnthologySap.Repositories;

public class GraduatedRepository(AppDbContext context)
    : Repository(context), IGraduatedRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base.Query()
            .WhereStudentTypes()
            .WhereGraduatedStudents();
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