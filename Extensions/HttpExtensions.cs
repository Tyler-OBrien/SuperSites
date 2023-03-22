using System.Net.Http.Json;
using System.Text.Json;
using CloudflareWorkerBundler.Models.CloudflareAPI;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Extensions;

public static class HttpExtensions
{
    public static async Task<TOut?> PostAsJsonAsync<TIn, TOut>(this HttpClient client, string? requestUri, TIn value,
        CancellationToken token)
        where TOut : class
    {
        var response = await client.PostAsJsonAsync(requestUri, value, token);
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            var rawString = await response.Content.ReadAsStringAsync(token);


            if (string.IsNullOrWhiteSpace(rawString) == false)
            {
                var output = JsonSerializer.Deserialize<TOut>(rawString);
                return output;
            }
        }

        return null;
    }


    public static async Task<ApiResponseBase?> ProcessHttpResponseAsync(this HttpResponseMessage httpResponse,
        string assetName, ILogger logger)
    {
        var response = await ProcessHttpResponseAsync<object?, object>(httpResponse, assetName, logger);
        return response;
    }


    public static async Task<ApiResponse<TResult[], TResultInfo>?> ProcessHttpResponseAsyncList<TResult, TResultInfo>(
        this HttpResponseMessage httpResponse, string assetName, ILogger logger)
    {
        try
        {
            var rawString = await httpResponse.Content.ReadAsStringAsync();


            if (string.IsNullOrWhiteSpace(rawString))
            {
                logger.LogCritical(
                    $"Could not get response {assetName} from API, API returned nothing, Status Code: {httpResponse.StatusCode}");
                return null;
            }

            var response = JsonSerializer.Deserialize<ApiResponse<TResult[], TResultInfo>>(rawString);

            if (response == null)
            {
                logger.LogCritical($"Could not get response {assetName} from API");
                return null;
            }

            if (response.Errors != null && response.Errors.Any())
            {
                foreach (var error in response.Errors)
                {
                    logger.LogCritical($"Error with {assetName}: {error}");
                }

                return null;
            }

            if (response.Messages != null && response.Messages.Any())
            {
                foreach (var message in response.Messages)
                {
                    logger.LogCritical($"API Returned Message with {assetName}: {message}");
                }
            }

            if (response.Success == false)
            {
                logger.LogCritical("Response Success did not indicitate success");
                return null;
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogCritical(ex, $"Unexpected HTTP Error: API Returned: {httpResponse?.StatusCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, $"Unexpected Error: API Returned: {httpResponse?.StatusCode}");
        }

        return null;
    }

    public static async Task<ApiResponse<TResult, TResultInfo>?> ProcessHttpResponseAsync<TResult, TResultInfo>(
        this HttpResponseMessage httpResponse, string assetName, ILogger logger)
    {
        try
        {
            var rawString = await httpResponse.Content.ReadAsStringAsync();


            if (string.IsNullOrWhiteSpace(rawString))
            {
                logger.LogCritical(
                    $"Could not get response {assetName} from API, API returned nothing, Status Code: {httpResponse.StatusCode}");
                return null;
            }

            var response = JsonSerializer.Deserialize<ApiResponse<TResult, TResultInfo>>(rawString);

            if (response == null)
            {
                logger.LogCritical($"Could not get response {assetName} from API");
                return null;
            }

            if (response.Errors != null && response.Errors.Any())
            {
                foreach (var error in response.Errors)
                {
                    logger.LogCritical($"Error with {assetName}: {error}");
                }

                return null;
            }

            if (response.Messages != null && response.Messages.Any())
            {
                foreach (var message in response.Messages)
                {
                    logger.LogCritical($"API Returned Message with {assetName}: {message}");
                }
            }

            if (response.Success == false)
            {
                logger.LogCritical("Response Success did not indicitate success");
                return null;
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogCritical(ex, $"Unexpected HTTP Error: API Returned: {httpResponse?.StatusCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, $"Unexpected Error: API Returned: {httpResponse?.StatusCode}");
        }

        return null;
    }
}