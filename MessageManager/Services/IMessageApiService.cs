// ===============================
// MessageManager/Services/IMessageApiService.cs - 返信済み機能追加版
// ===============================
using MessageManager.Models;
using System;
using System.Threading.Tasks;

namespace MessageManager.Services
{
    public interface IMessageApiService : IDisposable
    {
        // メッセージ関連
        Task<MessageListResponse> GetAllReceivedMessagesAsync(int? deptId);
        Task<SentMessageListResponse> GetAllSentMessagesAsync(int? deptId);
        Task<MessageDetailResponse> GetMessageDetailAsync(int messageId, int? deptId);
        Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, int? fromDeptId);
        Task<ActionUpdateResponse> UpdateActionStatusAsync(ActionUpdateRequest request, int? deptId);
        Task<MessageDoneResponse> UpdateMessageDoneAsync(MessageDoneRequest request, int? deptId);

        // ★ 新規追加：返信送信時の元メッセージ状態更新
        /// <summary>
        /// 返信メッセージ送信（元メッセージのアクション状態を「返信済み」に更新）
        /// </summary>
        Task<SendMessageResponse> SendReplyMessageAsync(SendMessageRequest request, int? fromDeptId, int originalMessageId);

        // 部署関連
        Task<DepartmentApiResponse> GetDepartmentsAsync();

        // 部署メンテナンス機能
        Task<GetAllDepartmentMastersResponse> GetAllDepartmentMastersAsync();
        Task<UpdateDepartmentMasterResponse> UpdateDepartmentMasterAsync(UpdateDepartmentMasterRequest request);
    }
}
