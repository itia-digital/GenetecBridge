namespace Core.Data;

public record UpRecordValue
{
    public required string Id { get; init; }
    public required Guid GenetecGroup { get; init; }
    public required string? Name { get; init; }
    public required string? Campus { get; init; }
    public required string? LastName { get; init; }
    public required string? Email { get; init; }
    public required string? Phone { get; init; }

    public string FullName =>
        string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Name)
            ? Name ?? LastName ?? Id
            : $"{LastName} {Name}";
}