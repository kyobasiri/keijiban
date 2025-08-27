using keijiban.Configuration;
using keijiban.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; // ★System.Net.Http.Json NuGetパッケージが必要
using System.Text;
using System.Threading.Tasks;

namespace keijiban.Services
{
    /// <summary>
    /// バックエンドAPIとの通信を担うサービスの具象実装。
    /// </summary>
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<ApiService> _logger;

        /// <summary>
        /// ApiServiceの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="httpClientFactory">HTTPクライアントを生成するためのファクトリ。</param>
        /// <param name="apiSettingsOptions">API接続設定。</param>
        /// <param name="logger">ロガー。</param>
        public ApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettingsOptions, ILogger<ApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiSettings = apiSettingsOptions.Value;
            _logger = logger;

            // HttpClientのベースアドレスとタイムアウトを設定
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_apiSettings.Timeout);

            _logger.LogInformation("ApiService initialized. BaseUrl: {ApiBaseUrl}, Timeout: {ApiTimeout}s", _apiSettings.BaseUrl, _apiSettings.Timeout);
        }

        #region Public API Methods

        // 各APIメソッドは、対応するエンドポイントのURLを構築し、
        // 共通のヘルパーメソッド(GetAsync)を呼び出すだけのシンプルな形になります。

        public Task<ScheduleApiResponse> GetScheduleGroupDataAsync(DateTime startDate, int? departmentId = null)
        {
            var url = $"schedule/group?startDate={startDate:yyyy-MM-dd}";
            if (departmentId.HasValue) url += $"&departmentId={departmentId.Value}";
            return GetAsync<ScheduleApiResponse>(url, new ScheduleApiResponse { Success = false, Message = "スケジュールグループデータの取得に失敗しました。" });
        }

        public Task<ScheduleApiResponse> GetDepartmentScheduleDataAsync(DateTime startDate, int? departmentId = null)
        {
            var url = $"schedule/department?startDate={startDate:yyyy-MM-dd}";
            if (departmentId.HasValue) url += $"&departmentId={departmentId.Value}";
            return GetAsync<ScheduleApiResponse>(url, new ScheduleApiResponse { Success = false, Message = "部署スケジュールデータの取得に失敗しました。" });
        }

        public Task<ExtraScheduleDataApiResponse> GetExtraScheduleDataAsync(int? departmentId = null, DateTime? startDate = null)
        {
            var query = new Dictionary<string, string?>();
            if (departmentId.HasValue) query["departmentId"] = departmentId.Value.ToString();
            if (startDate.HasValue) query["startDate"] = startDate.Value.ToString("yyyy-MM-dd");
            var url = BuildUrlWithQuery("schedule/extra-data", query);
            return GetAsync<ExtraScheduleDataApiResponse>(url, new ExtraScheduleDataApiResponse { Success = false, Message = "その他スケジュールデータの取得に失敗しました。" });
        }

        public Task<DoctorAbsenceApiResponse> GetDoctorAbsencesAsync(DateTime startDate)
        {
            var url = $"schedule/doctor-absences?startDate={startDate:yyyy-MM-dd}";
            return GetAsync<DoctorAbsenceApiResponse>(url, new DoctorAbsenceApiResponse { Success = false, Message = "医師不在予定データの取得に失敗しました。" });
        }

        public Task<DepartmentListsApiResponse> GetDepartmentListsAsync()
        {
            return GetAsync<DepartmentListsApiResponse>("schedule/lists", new DepartmentListsApiResponse { Success = false, Message = "部署リストの取得に失敗しました。" });
        }

        public Task<InformationApiResponse> GetInformationAsync(int? departmentId = null)
        {
            var url = "schedule/information";
            if (departmentId.HasValue) url += $"?departmentId={departmentId.Value}";
            return GetAsync<InformationApiResponse>(url, new InformationApiResponse { Success = false, Message = "インフォメーションデータの取得に失敗しました。" });
        }

        public Task<GetActiveNoticesForDepartmentResponse> GetCombinedActiveNoticesForDepartmentsAsync(int? scheduleGroupDepartmentId, int? displayDepartmentId)
        {
            var query = new Dictionary<string, string?>();
            if (scheduleGroupDepartmentId.HasValue) query["scheduleGroupDepartmentId"] = scheduleGroupDepartmentId.Value.ToString();
            if (displayDepartmentId.HasValue) query["displayDepartmentId"] = displayDepartmentId.Value.ToString();
            var url = BuildUrlWithQuery("emergencynotice/departments/combined/active", query);
            return GetAsync<GetActiveNoticesForDepartmentResponse>(url, new GetActiveNoticesForDepartmentResponse { Success = false, Message = "統合緊急情報の取得に失敗しました。" });
        }

        public Task<MessageListApiResponse> GetSentMessagesAsync(int? fromDeptId, int limit = 10)
        {
            if (!fromDeptId.HasValue) return Task.FromResult(new MessageListApiResponse { Success = false, Message = "無効な送信部署IDが指定されました。" });
            var url = $"message/sent?fromDeptId={fromDeptId.Value}&limit={limit}";
            return GetAsync<MessageListApiResponse>(url, new MessageListApiResponse { Success = false, Message = "送信メッセージの取得に失敗しました。" });
        }

        public Task<MessageListApiResponse> GetReceivedMessagesAsync(int? toDeptId, int limit = 10)
        {
            if (!toDeptId.HasValue) return Task.FromResult(new MessageListApiResponse { Success = false, Message = "無効な受信部署IDが指定されました。" });
            var url = $"message/received?toDeptId={toDeptId.Value}&limit={limit}";
            return GetAsync<MessageListApiResponse>(url, new MessageListApiResponse { Success = false, Message = "受信メッセージの取得に失敗しました。" });
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// APIに対してGETリクエストを送信し、結果をデシリアライズする共通メソッド。
        /// </summary>
        /// <typeparam name="T">デシリアライズするレスポンスの型。</typeparam>
        /// <param name="requestUrl">リクエストする相対URL。</param>
        /// <param name="errorResponse">エラー時に返すデフォルトのレスポンスオブジェクト。</param>
        /// <returns>成功した場合はAPIからのレスポンス、失敗した場合は指定されたエラーレスポンス。</returns>
        private async Task<T> GetAsync<T>(string requestUrl, T errorResponse) where T : class
        {
            try
            {
                var fullUri = new Uri(_httpClient.BaseAddress!, requestUrl);
                _logger.LogInformation("Sending GET request to: {FullUri}", fullUri.ToString());
                _logger.LogInformation("Requesting GET: {RequestUrl}", requestUrl);

                // `GetFromJsonAsync` は、成功ステータスコードでない場合に例外をスローします。
                var result = await _httpClient.GetFromJsonAsync<T>(requestUrl);

                if (result == null)
                {
                    _logger.LogWarning("GET request to {RequestUrl} returned null.", requestUrl);
                    return errorResponse;
                }

                _logger.LogDebug("GET request to {RequestUrl} completed successfully.", requestUrl);
                return result;
            }
            catch (HttpRequestException ex)
            {
                // API呼び出し自体には成功したが、エラーコード(4xx, 5xx)が返ってきた場合など
                _logger.LogError(ex, "API call failed for GET {RequestUrl}. StatusCode: {StatusCode}", requestUrl, ex.StatusCode);
                return errorResponse;
            }
            catch (Exception ex)
            {
                // タイムアウト、ネットワークエラー、JSONデシリアライズ失敗など
                _logger.LogError(ex, "An unexpected error occurred during GET {RequestUrl}", requestUrl);
                return errorResponse;
            }
        }

        /// <summary>
        /// ベースURLとクエリパラメータから完全なURLを構築します。
        /// </summary>
        private string BuildUrlWithQuery(string baseUrl, Dictionary<string, string?> queryParams)
        {
            if (!queryParams.Any(kvp => kvp.Value != null))
            {
                return baseUrl;
            }

            var builder = new StringBuilder(baseUrl);
            builder.Append('?');

            var validParams = queryParams
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!)}");

            builder.Append(string.Join('&', validParams));

            return builder.ToString();
        }

        #endregion
    }
}
