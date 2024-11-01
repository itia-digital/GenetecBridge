using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Genetec.Services.Core;

public abstract class BaseService
{
    protected readonly ILogger Logger;
    protected readonly APISettings Settings;
    protected readonly HttpClient HttpClient;
    protected readonly AsyncRetryPolicy RetryPolicy;
    protected readonly JsonSerializerOptions JsonSerializerOptions;

    protected BaseService(ILogger logger, HttpClient httpClient, APISettings settings, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        HttpClient = httpClient;
        Settings = settings;
        Logger = logger;
        JsonSerializerOptions = jsonSerializerOptions
                                ?? new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                    WriteIndented = true
                                };

        RetryPolicy = Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Logger.LogWarning("Retry {retryCount} encountered an error: {exception.Message}",
                        retryCount, exception.Message);
                });
    }

    protected async Task<TResult?> ExecuteGetAsync<TResult>(string sourcePath)
    {
        HttpClient.DefaultRequestHeaders.Clear();
        SetCommonHeaders();

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            HttpResponseMessage response = await HttpClient.GetAsync($"{Settings.BaseUrl}{sourcePath}");
            await HandleNonSuccessResponseAsync(response);
            
            var content = await response.Content.ReadAsStringAsync();
            BaseResponse<TResult>? result = JsonSerializer.Deserialize<BaseResponse<TResult>>(content, JsonSerializerOptions);
            return result != null ? result.Result : default;
        });
    }

    protected async Task<TResult?> ExecutePostAsync<TResult, TRequest>(string sourcePath, TRequest requestBody)
    {
        HttpClient.DefaultRequestHeaders.Clear();
        SetCommonHeaders();

        StringContent content = new(JsonSerializer.Serialize(requestBody, JsonSerializerOptions),
            System.Text.Encoding.UTF8, "application/json");

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            HttpResponseMessage response = await HttpClient.PostAsync($"{Settings.BaseUrl}{sourcePath}", content);
            await HandleNonSuccessResponseAsync(response);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            BaseResponse<TResult>? result = JsonSerializer.Deserialize<BaseResponse<TResult>>(responseBody, JsonSerializerOptions);
            return result != null ? result.Result : default;
        });
    }

    protected async Task<TResult?> ExecutePutAsync<TResult, TRequest>(string sourcePath, TRequest requestBody)
    {
        HttpClient.DefaultRequestHeaders.Clear();
        SetCommonHeaders();

        StringContent content = new(JsonSerializer.Serialize(requestBody, JsonSerializerOptions),
            System.Text.Encoding.UTF8, "application/json");

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            HttpResponseMessage response = await HttpClient.PutAsync($"{Settings.BaseUrl}{sourcePath}", content);
            await HandleNonSuccessResponseAsync(response);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            BaseResponse<TResult>? result = JsonSerializer.Deserialize<BaseResponse<TResult>>(responseBody, JsonSerializerOptions);
            return result != null ? result.Result : default;
        });
    }

    protected async Task<TResult?> ExecuteDeleteAsync<TResult>(string requestUrl)
    {
        HttpClient.DefaultRequestHeaders.Clear();
        SetCommonHeaders();

        return await RetryPolicy.ExecuteAsync(async () =>
        {
            HttpResponseMessage response = await HttpClient.DeleteAsync($"{Settings.BaseUrl}{requestUrl}");
            await HandleNonSuccessResponseAsync(response);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            BaseResponse<TResult>? result = JsonSerializer.Deserialize<BaseResponse<TResult>>(responseBody, JsonSerializerOptions);
            return result != null ? result.Result : default;
        });
    }

    private void SetCommonHeaders()
    {
        HttpClient.DefaultRequestHeaders.Add("Authorization",
            "Basic " + Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{Settings.Username}:{Settings.Password}")));
        HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    private async Task HandleNonSuccessResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogError(
                "HTTP Request failed. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}",
                response.StatusCode,
                response.ReasonPhrase
            );

            var details = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(details))
            {
                Logger.LogError(details);
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new HttpRequestException("Resource not found.");
                case HttpStatusCode.Unauthorized:
                    throw new HttpRequestException("Unauthorized access. Check your credentials.");
                case HttpStatusCode.Forbidden:
                    throw new HttpRequestException("Access is forbidden. Verify your permissions.");
                case HttpStatusCode.InternalServerError:
                    throw new HttpRequestException("Server error occurred. Try again later.");
                default:
                    response.EnsureSuccessStatusCode(); // Throws exception for other non-success codes
                    break;
            }
        }
    }
}