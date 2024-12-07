namespace Genetec.Services.Models;

public record CreateUserRequest(string Name, string FirstName, string LastName, string? Email);