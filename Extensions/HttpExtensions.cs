using CloudflareWorkerBundler.Models.CloudflareAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Extensions
{
    public static class HttpExtensions
    {
        public static async Task<TOut?> PostAsJsonAsync<TIn, TOut>(this HttpClient client, string? requestUri, TIn value, CancellationToken token)
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

        public static async Task<APIResponse?> ProcessHttpResponseAsync(this HttpResponseMessage httpResponse, string assetName)
        {
            try
            {
                var rawString = await httpResponse.Content.ReadAsStringAsync();


                if (string.IsNullOrWhiteSpace(rawString))
                {
                    Console.WriteLine(
                        $"Could not get response {assetName} from API, API returned nothing, Status Code: {httpResponse.StatusCode}");
                    return null;
                }

                var response = JsonSerializer.Deserialize<APIResponse>(rawString);

                if (response == null)
                {
                    Console.WriteLine($"Could not get response {assetName} from API");
                    return null;
                }

                if (response.Errors != null && response.Errors.Any())
                {
                    foreach (var error in response.Errors)
                    {
                        Console.WriteLine($"Error with {assetName}: {error}");

                    }
                    return null;
                }
                if (response.Messages != null && response.Messages.Any())
                {
                    foreach (var message in response.Messages)
                    {
                        Console.WriteLine($"API Returned Message with {assetName}: {message}");
                    }
                }

                if (response.Success == false)
                {
                    Console.WriteLine("Response Success did not indicitate success");
                    return null;
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine($"Unexpected HTTP Error: API Returned: {httpResponse?.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine($"Unexpected Error: API Returned: {httpResponse?.StatusCode}");
            }

            return null;
        }


        public static async Task<T?> ProcessHttpResponseAsync<T>(this HttpResponseMessage httpResponse, string assetName)
            where T : class
        {
            try
            {
                var rawString = await httpResponse.Content.ReadAsStringAsync();


                if (string.IsNullOrWhiteSpace(rawString))
                {
                    Console.WriteLine(
                        $"Could not get response {assetName} from API, API returned nothing, Status Code: {httpResponse.StatusCode}");
                    return null;
                }

                var response = JsonSerializer.Deserialize<APIResponse<T>>(rawString);

                if (response == null)
                {
                    Console.WriteLine($"Could not get response {assetName} from API");
                    return null;
                }

                if (response.Errors.Any())
                {
                    foreach (var error in response.Errors)
                    {
                        Console.WriteLine($"Error with {assetName}: {error}");

                    }
                    return null;
                }
                if (response.Messages.Any())
                {
                    foreach (var message in response.Messages)
                    {
                        Console.WriteLine($"API Returned Message with {assetName}: {message}");
                    }
                }

                if (response.Result == null)
                {
                    Console.WriteLine($"Unknown error with {assetName}");
                    return null;
                }

                return response.Result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine($"Unexpected HTTP Error: API Returned: {httpResponse?.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine($"Unexpected Error: API Returned: {httpResponse?.StatusCode}");
            }

            return null;
        }
    }
}
