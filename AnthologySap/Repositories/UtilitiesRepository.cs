using AnthologySap.Models;
using Core.Data.Extensions;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AnthologySap.Repositories;

public class UtilitiesRepository(AppDbContext context) : IUtilitiesRepository
{
    public async Task<List<string>> GetActiveRecordsAsync(DateTime? updatedAt = null, CancellationToken cancellationToken = default)
    {
        var result = await context.VUsuariosUnificados
            .ConditionalWhere(updatedAt != null,
                e => e.Lastupddttm!.Value.Date == updatedAt!.Value.Date)
            .Where(i =>
                context.VUsuariosUnificados
                    .Where(a => a.Emplid == i.Emplid)
                    .Any(a =>
                        a.StatusField == "A"
                        || a.StatusField == "AC"
                        || (a.StatusField == "DM" && a.ProgStatus == "Egresado")
                        || (a.StatusField == "CM" && a.ProgStatus == "CRED")
                        || (a.StatusField == "SP"
                            && (a.ProgStatus == "Egresado" || a.ProgStatus == "EGRP"))
                    )
            )
            .Select(i => i.Emplid.Trim())
            .Distinct()
            .ToListAsync(cancellationToken);

        return result;
    }
}