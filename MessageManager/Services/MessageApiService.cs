// Services/MessageApiService.cs (リファクタリング後)
using MessageManager.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MessageManager.Configuration; // ★ 追加
using Microsoft.Extensions.Options; // ★ 追加

namespace MessageManager.Services
{
    public class MessageApiService : IMessageApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MessageApiService> _logger;

        public MessageApiService(HttpClient httpClient, ILogger<MessageApiService> logger, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _logger = logger;

            // DIから注入された設定値を取得
            var settings = apiSettings.Value;

            _httpClient.BaseAddress = new Uri(settings.BaseUrl + "/");
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.Timeout);

            _logger.LogInformation($"MessageApiService initialized with BaseUrl: {settings.BaseUrl}");
        }

        public Task<MessageListResponse> GetAllReceivedMessagesAsync(int? deptId)
        {
            if (deptId == null)
                return Task.FromResult(new MessageListResponse { Success = false, Message = "部署IDが指定されていません" });

            var url = $"message/received/all?deptId={deptId}";
            return ApiServiceHelper.GetAsync<MessageListResponse>(_httpClient, url, _logger);
        }

        public Task<SentMessageListResponse> GetAllSentMessagesAsync(int? deptId)
        {
            if (deptId == null)
                return Task.FromResult(new SentMessageListResponse { Success = false, Message = "部署IDが指定されていません" });

            var url = $"message/sent/all?deptId={deptId}";
            return ApiServiceHelper.GetAsync<SentMessageListResponse>(_httpClient, url, _logger);
        }

        public Task<MessageDetailResponse> GetMessageDetailAsync(int messageId, int? deptId)
        {
            if (deptId == null)
                return Task.FromResult(new MessageDetailResponse { Success = false, Message = "部署IDが指定されていません" });

            var url = $"message/{messageId}?deptId={deptId}";
            return ApiServiceHelper.GetAsync<MessageDetailResponse>(_httpClient, url, _logger);
        }

        public Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, int? fromDeptId)
        {
            var url = $"message/send?fromDeptId={fromDeptId}";
            return ApiServiceHelper.PostAsync<SendMessageResponse, SendMessageRequest>(_httpClient, url, request, _logger);
        }

        public Task<SendMessageResponse> SendReplyMessageAsync(SendMessageRequest request, int? fromDeptId, int originalMessageId)
        {
            var url = $"message/send/reply?fromDeptId={fromDeptId}&originalMessageId={originalMessageId}";
            return ApiServiceHelper.PostAsync<SendMessageResponse, SendMessageRequest>(_httpClient, url, request, _logger);
        }

        public Task<ActionUpdateResponse> UpdateActionStatusAsync(ActionUpdateRequest request, int? deptId)
        {
            var url = $"message/action?deptId={deptId}";
            return ApiServiceHelper.PostAsync<ActionUpdateResponse, ActionUpdateRequest>(_httpClient, url, request, _logger);
        }

        public Task<MessageDoneResponse> UpdateMessageDoneAsync(MessageDoneRequest request, int? deptId)
        {
            if (deptId == null)
                return Task.FromResult(new MessageDoneResponse { Success = false, Message = "部署IDが指定されていません" });

            var url = $"message/done?deptId={deptId}";
            return ApiServiceHelper.PostAsync<MessageDoneResponse, MessageDoneRequest>(_httpClient, url, request, _logger);
        }

        public Task<DepartmentApiResponse> GetDepartmentsAsync()
        {
            var url = "schedule/departments";
            return ApiServiceHelper.GetAsync<DepartmentApiResponse>(_httpClient, url, _logger);
        }

        public Task<GetAllDepartmentMastersResponse> GetAllDepartmentMastersAsync()
        {
            return ApiServiceHelper.GetAsync<GetAllDepartmentMastersResponse>(_httpClient, "schedule/department-masters", _logger);
        }

        public Task<UpdateDepartmentMasterResponse> UpdateDepartmentMasterAsync(UpdateDepartmentMasterRequest request)
        {
            return ApiServiceHelper.PutAsync<UpdateDepartmentMasterResponse, UpdateDepartmentMasterRequest>(
                _httpClient, "schedule/department-master", request, _logger);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
