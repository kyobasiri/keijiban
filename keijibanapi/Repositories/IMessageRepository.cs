// ====================================
// Repositories/IMessageRepository.cs
// ====================================
using keijibanapi.DataTransferObjects;
using keijibanapi.Models;


namespace keijibanapi.Repositories
{
    /// <summary>
    /// メッセージ関連データへのアクセスを定義します。
    /// </summary>
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetSentMessagesAsync(int fromDeptId, int limit);
        Task<IEnumerable<ReceivedMessageDto>> GetReceivedMessagesAsync(int toDeptId, int limit);
        Task<Message?> GetMessageByIdAsync(int messageId);
        Task<IEnumerable<MessageRecipient>> GetRecipientsByMessageIdAsync(int messageId);
        Task<IEnumerable<MessageAction>> GetActionsByMessageIdAsync(int messageId);
        Task MarkAsReadAsync(int messageId, int deptId);
        Task<long> CreateMessageAsync(SendMessageRequest request, int fromDeptId, int? originalMessageId);
        Task UpdateActionStatusToRepliedAsync(int originalMessageId, int replierDeptId);
        Task UpdateActionStatusAsync(ActionUpdateRequest request, int deptId);
        Task<IEnumerable<Message>> GetAllSentMessagesAsync(int fromDeptId);
        Task<bool> UpdateMessageDoneAsync(int messageId, bool isDone);
    }
}
