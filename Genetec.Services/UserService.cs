using Genetec.Services.Core;
using Genetec.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genetec.Services;

public interface IUserService
{
    Task<GetUserResponse?> GetUserAsync(string userId);
    Task<GetUserResponse?> PostUserAsync(CreateUserRequest request);
}

public class UserService(HttpClient httpClient, IOptions<APISettings> settings, ILogger<UserService> logger)
    : BaseService(logger, httpClient, settings.Value), IUserService
{
    public async Task<GetUserResponse?> GetUserAsync(string userId) 
        => await ExecuteGetAsync<GetUserResponse>($"/users/{userId}");

    public async Task<GetUserResponse?> PostUserAsync(CreateUserRequest request)
    {
        string url = "/entity?q=entity=NewEntity(User),Name={{name}},FirstName={{firstName}},LastName={{lastName}},EmailAddress={{email}},Guid";
        return await ExecutePostAsync<GetUserResponse, CreateUserRequest>("/users", request);
    }
}