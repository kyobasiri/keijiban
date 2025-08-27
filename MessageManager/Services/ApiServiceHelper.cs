// Services/ApiServiceHelper.cs

using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageManager.Services
{
    public static class ApiServiceHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// GETリクエストを非同期に実行します。
        /// </summary>
        public static async Task<TResponse> GetAsync<TResponse>(
            HttpClient client,
            string url,
            ILogger logger)
            where TResponse : class, IApiResponse, new()
        {
            try
            {
                logger.LogDebug($"Requesting GET: {client.BaseAddress}{url}");
                var response = await client.GetAsync(url);
                return await ProcessResponse<TResponse>(response, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception in GET request to {url}");
                return new TResponse { Success = false, Message = $"通信エラー: {ex.Message}" };
            }
        }

        /// <summary>
        /// POSTリクエストを非同期に実行します。
        /// </summary>
        public static async Task<TResponse> PostAsync<TResponse, TRequest>(
            HttpClient client,
            string url,
            TRequest requestData,
            ILogger logger)
            where TResponse : class, IApiResponse, new()
        {
            try
            {
                logger.LogDebug($"Requesting POST: {client.BaseAddress}{url}");
                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                return await ProcessResponse<TResponse>(response, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception in POST request to {url}");
                return new TResponse { Success = false, Message = $"通信エラー: {ex.Message}" };
            }
        }

        /// <summary>
        /// PUTリクエストを非同期に実行します。
        /// </summary>
        public static async Task<TResponse> PutAsync<TResponse, TRequest>(
            HttpClient client,
            string url,
            TRequest requestData,
            ILogger logger)
            where TResponse : class, IApiResponse, new()
        {
            try
            {
                logger.LogDebug($"Requesting PUT: {client.BaseAddress}{url}");
                var jsonContent = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, content);
                return await ProcessResponse<TResponse>(response, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception in PUT request to {url}");
                return new TResponse { Success = false, Message = $"通信エラー: {ex.Message}" };
            }
        }

        /// <summary>
        /// PATCHリクエストを非同期に実行します。
        /// </summary>
        public static async Task<TResponse> PatchAsync<TResponse, TRequest>(
            HttpClient client,
            string url,
            TRequest requestData,
            ILogger logger)
            where TResponse : class, IApiResponse, new()
        {
            try
            {
                logger.LogDebug($"Requesting PATCH: {client.BaseAddress}{url}");
                var jsonContent = JsonSerializer.Serialize(requestData, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync(url, content);
                return await ProcessResponse<TResponse>(response, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception in PATCH request to {url}");
                return new TResponse { Success = false, Message = $"通信エラー: {ex.Message}" };
            }
        }

        /// <summary>
        /// HttpResponseMessageを処理し、指定された型にデシリアライズします。
        /// </summary>
        private static async Task<TResponse> ProcessResponse<TResponse>(HttpResponseMessage response, ILogger logger)
            where TResponse : class, IApiResponse, new()
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
                if (result == null)
                {
                    logger.LogWarning($"JSON deserialization resulted in null for response: {responseContent}");
                    return new TResponse { Success = false, Message = "レスポンスの解析に失敗しました。" };
                }
                return result;
            }
            else
            {
                logger.LogWarning($"API call failed with status code {response.StatusCode}: {responseContent}");
                return new TResponse { Success = false, Message = $"API呼び出しに失敗しました: {response.StatusCode}" };
            }
        }
    }
}
