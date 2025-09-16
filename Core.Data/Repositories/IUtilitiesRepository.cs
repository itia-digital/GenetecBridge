namespace Core.Data.Repositories;

public interface IUtilitiesRepository
{
    Task<List<string>> GetActiveRecordsAsync(DateTime? updatedAt = null, CancellationToken cancellationToken = default);
}