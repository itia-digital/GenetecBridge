﻿namespace Core.Data;

public record UpRecordValue
{
    public required Guid GenetecGroup { get; init; }
    public required string Id { get; init; }
    public required string? Name { get; init; }
    public required string? Campus { get; init; }
    public required string? LastName { get; init; }
    public required string? Email { get; init; }
    public string? Phone { get; init; }
    public required string? PositionOrProgram { get; init; }
    public required string? Type { get; init; }
    public required bool IsActive { get; init; }

    public string FullName =>
        string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Name)
            ? Name ?? LastName ?? Id
            : $"{Name} {LastName}";

    public override string ToString()
    {
        return
            $"{{ {nameof(Id)}: {Id}, {nameof(GenetecGroup)}: {GenetecGroup}, {nameof(Name)}: {Name}, {nameof(Campus)}: {Campus}, {nameof(LastName)}: {LastName}, {nameof(Email)}: {Email}, {nameof(Phone)}: {Phone} }}";
    }
}