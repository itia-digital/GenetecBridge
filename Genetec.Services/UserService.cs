using Genetec.Services.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genetec.Services;

public interface IUserService
{
    Task<UserResponse?> GetUserAsync(string userId);
}

public class UserService(HttpClient httpClient, IOptions<APISettings> settings, ILogger<UserService> logger)
    : BaseService(logger, httpClient, settings.Value), IUserService
{
    public async Task<UserResponse?> GetUserAsync(string userId) 
        => await ExecuteGetAsync<UserResponse>($"/users/{userId}");
}