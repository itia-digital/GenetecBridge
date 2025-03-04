using Core.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;

namespace UP.Data.Repositories;

public interface IUtilitiesRepository
{
    Task<List<string>> GetActiveRecordsAsync(DateTime? updatedAt = null, CancellationToken cancellationToken = default);
}

public class UtilitiesRepository(UpDbContext context) : IUtilitiesRepository
{
    public async Task<List<string>> GetActiveRecordsAsync(DateTime? updatedAt = null, CancellationToken cancellationToken = default)
    {
        var result = await context.PsUpIdGralTVws
            .ConditionalWhere(updatedAt != null,
                e => e.Lastupddttm!.Value.Date == updatedAt!.Value.Date)
            .Where(i =>
                context.PsUpIdGralTVws
                    .Where(a => a.Emplid == i.Emplid)
                    .Any(a =>
                        a.StatusField == "A"
                        || a.StatusField == "AC"
                        || (a.StatusField == "DM" && a.ProgReason == "EGR")
                        || (a.StatusField == "CM" && a.ProgReason == "CRED")
                        || (a.StatusField == "SP"
                            && (a.ProgReason == "EGR" || a.ProgReason == "EGRP"))
                    )
            )
            .Select(i => i.Emplid.Trim())
            .Distinct()
            .ToListAsync(cancellationToken);

        return result;
    }
}