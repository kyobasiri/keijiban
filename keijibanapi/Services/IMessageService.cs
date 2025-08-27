// ===================================
// Services/IMessageService.cs
// ===================================
using keijibanapi.Models;

namespace keijibanapi.Services
{
    public interface IMessageService
    {
        Task<MessageListResponse> GetSentMessagesAsync(int fromDeptId, int limit = 10);
        Task<MessageListResponse> GetReceivedMessagesAsync(int toDeptId, int limit = 10);
        Task<MessageDetailResponse> GetMessageDetailAsync(int messageId, int deptId);
        Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, int fromDeptId);
        Task<SendMessageResponse> SendReplyMessageAsync(SendMessageRequest request, int fromDeptId, int originalMessageId);
        Task<ActionUpdateResponse> UpdateActionStatusAsync(ActionUpdateRequest request, int deptId);
        Task<MessageListResponse> GetAllReceivedMessagesAsync(int deptId);
        Task<SentMessageListResponse> GetAllSentMessagesAsync(int fromDeptId);
        Task<MessageDoneResponse> UpdateMessageDoneAsync(MessageDoneRequest request, int deptId);
    }
}
