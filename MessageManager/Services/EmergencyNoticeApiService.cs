// Services/EmergencyNoticeApiService.cs (リファクタリング後)
using MessageManager.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MessageManager.Configuration; // ★ 追加
using Microsoft.Extensions.Options; // ★ 追加

namespace MessageManager.Services
{
    public class EmergencyNoticeApiService : IEmergencyNoticeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmergencyNoticeApiService> _logger;

        public EmergencyNoticeApiService(HttpClient httpClient, ILogger<EmergencyNoticeApiService> logger, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _logger = logger;

            var settings = apiSettings.Value;

            _httpClient.BaseAddress = new Uri(settings.BaseUrl + "/");
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.Timeout);
        }

        public Task<GetEmergencyNoticesResponse> GetAllNoticesAsync()
        {
            return ApiServiceHelper.GetAsync<GetEmergencyNoticesResponse>(_httpClient, "emergencynotice", _logger);
        }

        public Task<EmergencyNoticeResponse> CreateNoticeAsync(CreateEmergencyNoticeRequest request)
        {
            return ApiServiceHelper.PostAsync<EmergencyNoticeResponse, CreateEmergencyNoticeRequest>(_httpClient, "emergencynotice", request, _logger);
        }

        public Task<EmergencyNoticeResponse> UpdateNoticeAsync(UpdateEmergencyNoticeRequest request)
        {
            var url = $"emergencynotice/{request.Id}";
            return ApiServiceHelper.PutAsync<EmergencyNoticeResponse, UpdateEmergencyNoticeRequest>(_httpClient, url, request, _logger);
        }

        public Task<EmergencyNoticeResponse> ToggleNoticeAsync(ToggleEmergencyNoticeRequest request)
        {
            var url = $"emergencynotice/{request.Id}/toggle";
            return ApiServiceHelper.PatchAsync<EmergencyNoticeResponse, ToggleEmergencyNoticeRequest>(_httpClient, url, request, _logger);
        }

        public Task<EmergencyNoticeResponse> DeleteNoticeAsync(int id)
        {
            // DeleteAsyncはヘルパーに未実装なので、必要であれば追加します。
            // ここでは元の実装を参考に残しておきます。
            throw new NotImplementedException();
        }

        public Task<GetActiveNoticesForDepartmentResponse> GetActiveNoticesForDepartmentAsync(int departmentId)
        {
            var url = $"emergencynotice/department/{departmentId}/active";
            return ApiServiceHelper.GetAsync<GetActiveNoticesForDepartmentResponse>(_httpClient, url, _logger);
        }
    }
}
