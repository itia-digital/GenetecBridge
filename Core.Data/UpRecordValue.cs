namespace Core.Data;

public record UpRecordValue
{
    public string Id { get; init; }
    public Guid GenetecGroup { get; init; }
    public string? Name { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }

    public string FullName =>
        string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Name)
            ? Name ?? LastName ?? Id
            : $"{LastName} {Name}";
}