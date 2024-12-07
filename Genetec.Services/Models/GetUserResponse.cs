namespace Genetec.Services.Models;

public record GetUserResponse
{
    public string EmailAddress { get; set; } = string.Empty;
    public string UserStatus { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<Guid> UserGroups { get; set; } = [];
}