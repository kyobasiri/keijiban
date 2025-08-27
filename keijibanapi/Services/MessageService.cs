// ====================================
// Services/MessageService.cs - リファクタリング後
// ====================================
using keijibanapi.Models;
using keijibanapi.Repositories;

namespace keijibanapi.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IDepartmentService _departmentService; // 部署名取得のために利用
        private readonly ILogger<MessageService> _logger;

        public MessageService(IMessageRepository messageRepository, IDepartmentService departmentService, ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _departmentService = departmentService;
            _logger = logger;
        }

        public async Task<MessageListResponse> GetSentMessagesAsync(int fromDeptId, int limit = 10)
        {
            try
            {
                var messagesData = await _messageRepository.GetSentMessagesAsync(fromDeptId, limit);
                var departmentNames = await _departmentService.GetDepartmentNamesDictionaryAsync();

                var messages = messagesData.Select(msg => new MessageListItem
                {
                    MessageId = msg.MessageId,
                    Subject = msg.Subject,
                    FromDeptName = departmentNames.GetValueOrDefault(msg.FromDeptId, "不明な部署"),
                    ToDeptNames = string.Join(", ", msg.Recipients.Select(r => departmentNames.GetValueOrDefault(r.ToDeptId, "不明な部署"))),
                    Priority = msg.Priority,
                    CreatedAt = msg.CreatedAt,
                    DueDate = msg.DueDate,
                    RequiresAction = msg.RequiresAction,
                    ReadCount = msg.ReadCount,
                    TotalRecipients = msg.RecipientCount,
                    IsDone = msg.MessageDone == 1,
                    OriginalMessageId = msg.OriginalMessageId,
                    // 表示用プロパティの組み立てはサービス層の役割
                    PriorityDisplay = GetPriorityDisplay(msg.Priority),
                    DateDisplay = msg.CreatedAt.ToString("M/d"),
                    DueDateDisplay = msg.DueDate?.ToString("M/d") ?? "",
                    ReadProgress = $"{msg.ReadCount}/{msg.RecipientCount}",
                    StatusDisplay = GetStatusDisplay(msg.ReadCount, msg.RecipientCount, msg.RequiresAction, msg.ActionRequiredCount, msg.ActionCompletedCount),
                }).ToList();

                return new MessageListResponse { Messages = messages, Success = true, Message = "送信メッセージを正常に取得しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sent messages for department {DeptId}", fromDeptId);
                return new MessageListResponse { Success = false, Message = $"送信メッセージの取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<MessageListResponse> GetReceivedMessagesAsync(int toDeptId, int limit = 10)
        {
            try
            {
                var messagesData = await _messageRepository.GetReceivedMessagesAsync(toDeptId, limit);
                var departmentNames = await _departmentService.GetDepartmentNamesDictionaryAsync();

                var messages = messagesData.Select(dto => new MessageListItem
                {
                    MessageId = dto.MessageId,
                    Subject = dto.Subject,
                    FromDeptName = departmentNames.GetValueOrDefault(dto.FromDeptId, "不明な部署"),
                    Priority = dto.Priority,
                    DueDate = dto.DueDate,
                    CreatedAt = dto.CreatedAt,
                    IsRead = dto.IsRead, // DTOから直接 IsRead を取得
                    RequiresAction = dto.RequiresAction,
                    ActionStatus = dto.ActionStatus,
                    IsDone = dto.MessageDone == 1,
                    OriginalMessageId = dto.OriginalMessageId,
                    PriorityDisplay = GetPriorityDisplay(dto.Priority),
                    DateDisplay = dto.CreatedAt.ToString("M/d"),
                    DueDateDisplay = dto.DueDate?.ToString("M/d") ?? "",
                    StatusDisplay = GetReceivedStatusDisplay(dto.IsRead, dto.RequiresAction) // DTOのIsReadを使用
                }).ToList();

                return new MessageListResponse { Messages = messages, Success = true, Message = "受信メッセージを正常に取得しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving received messages for department {DeptId}", toDeptId);
                return new MessageListResponse { Success = false, Message = $"受信メッセージの取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<MessageDetailResponse> GetMessageDetailAsync(int messageId, int deptId)
        {
            try
            {
                var message = await _messageRepository.GetMessageByIdAsync(messageId);
                if (message == null)
                    return new MessageDetailResponse { Success = false, Message = "メッセージが見つかりません" };

                var recipients = await _messageRepository.GetRecipientsByMessageIdAsync(messageId);
                var actions = await _messageRepository.GetActionsByMessageIdAsync(messageId);
                var departmentNames = await _departmentService.GetDepartmentNamesDictionaryAsync();

                message.Recipients = recipients.ToList();
                message.Actions = actions.ToList();

                foreach (var recipient in message.Recipients)
                {
                    recipient.ToDeptName = departmentNames.GetValueOrDefault(recipient.ToDeptId, "不明な部署");
                }
                foreach (var action in message.Actions)
                {
                    action.DeptName = departmentNames.GetValueOrDefault(action.DeptId, "不明な部署");
                }

                var detail = new MessageDetail
                {
                    MessageId = message.MessageId,
                    Subject = message.Subject,
                    Content = message.Content,
                    FromDeptName = departmentNames.GetValueOrDefault(message.FromDeptId, "不明な部署"),
                    Priority = message.Priority,
                    Category = message.Category,
                    CreatedAt = message.CreatedAt,
                    DueDate = message.DueDate,
                    RequiresAction = message.RequiresAction,
                    IsDone = message.MessageDone == 1,
                    OriginalMessageId = message.OriginalMessageId,
                    Recipients = message.Recipients,
                    Actions = message.Actions,
                    PriorityDisplay = GetPriorityDisplay(message.Priority),
                    DateDisplay = message.CreatedAt.ToString("yyyy年M月d日 HH:mm"),
                    DueDateDisplay = message.DueDate?.ToString("yyyy年M月d日") ?? "",
                };

                // 閲覧したら既読にする
                await _messageRepository.MarkAsReadAsync(messageId, deptId);

                return new MessageDetailResponse { MessageDetail = detail, Success = true, Message = "メッセージ詳細を正常に取得しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message detail for message {MessageId}", messageId);
                return new MessageDetailResponse { Success = false, Message = $"メッセージ詳細の取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, int fromDeptId)
        {
            try
            {
                var messageId = await _messageRepository.CreateMessageAsync(request, fromDeptId, null);
                return new SendMessageResponse { MessageId = (int)messageId, Success = true, Message = "メッセージを正常に送信しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from department {FromDeptId}", fromDeptId);
                return new SendMessageResponse { Success = false, Message = $"メッセージの送信に失敗しました: {ex.Message}" };
            }
        }

        public async Task<SendMessageResponse> SendReplyMessageAsync(SendMessageRequest request, int fromDeptId, int originalMessageId)
        {
            try
            {
                var messageId = await _messageRepository.CreateMessageAsync(request, fromDeptId, originalMessageId);
                // 元メッセージのアクションステータスを更新
                await _messageRepository.UpdateActionStatusToRepliedAsync(originalMessageId, fromDeptId);

                return new SendMessageResponse { MessageId = (int)messageId, Success = true, Message = "返信メッセージを正常に送信しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reply message from department {FromDeptId}", fromDeptId);
                return new SendMessageResponse { Success = false, Message = $"メッセージの送信に失敗しました: {ex.Message}" };
            }
        }

        public async Task<ActionUpdateResponse> UpdateActionStatusAsync(ActionUpdateRequest request, int deptId)
        {
            try
            {
                await _messageRepository.UpdateActionStatusAsync(request, deptId);
                return new ActionUpdateResponse { Success = true, Message = "アクション状況を正常に更新しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating action status for message {MessageId}", request.MessageId);
                return new ActionUpdateResponse { Success = false, Message = $"アクション状況の更新に失敗しました: {ex.Message}" };
            }
        }

        public async Task<MessageListResponse> GetAllReceivedMessagesAsync(int deptId)
        {
            // limitを大きくして全件取得を模擬
            return await GetReceivedMessagesAsync(deptId, 1000);
        }

        public async Task<SentMessageListResponse> GetAllSentMessagesAsync(int fromDeptId)
        {
            try
            {
                var messagesData = await _messageRepository.GetAllSentMessagesAsync(fromDeptId);
                var departmentNames = await _departmentService.GetDepartmentNamesDictionaryAsync();

                var messages = messagesData.Select(msg =>
                {
                    var toDeptIdsString = string.Join(",", msg.Recipients.Select(r => r.ToDeptId));
                    var toDeptNames = string.Join(", ",
                        toDeptIdsString.Split(',')
                            .Where(id => !string.IsNullOrEmpty(id) && int.TryParse(id, out _))
                            .Select(id => departmentNames.GetValueOrDefault(int.Parse(id), "不明な部署"))
                    );

                    return new SentMessageListApiItem
                    {
                        MessageId = msg.MessageId,
                        Subject = msg.Subject,
                        ToDeptNames = toDeptNames,
                        Priority = msg.Priority,
                        CreatedAt = msg.CreatedAt,
                        DueDate = msg.DueDate,
                        RequiresAction = msg.RequiresAction,
                        IsDone = msg.MessageDone == 1,
                        OriginalMessageId = msg.OriginalMessageId
                    };
                }).ToList();

                return new SentMessageListResponse { Messages = messages, Success = true, Message = "送信メッセージを正常に取得しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sent messages for department {DeptId}", fromDeptId);
                return new SentMessageListResponse { Success = false, Message = $"送信メッセージの取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<MessageDoneResponse> UpdateMessageDoneAsync(MessageDoneRequest request, int deptId)
        {
            try
            {
                var success = await _messageRepository.UpdateMessageDoneAsync(request.MessageId, request.IsDone);
                return new MessageDoneResponse
                {
                    Success = success,
                    Message = success ? "メッセージの完了状態を正常に更新しました" : "対象のメッセージが見つかりませんでした"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message done status for message {MessageId}", request.MessageId);
                return new MessageDoneResponse { Success = false, Message = $"メッセージ完了状態の更新に失敗しました: {ex.Message}" };
            }
        }

        // ヘルパーメソッド群 (これらはビジネスロジックの一部としてServiceに残ります)
        private static string GetPriorityDisplay(Priority priority) => priority switch { Priority.Urgent => "緊急", Priority.High => "重要", Priority.Normal => "通常", Priority.Low => "参考", _ => "通常" };
        private static string GetStatusDisplay(int readCount, int total, bool reqAction, int actReq, int actComp) => reqAction ? $"対応状況: {actComp}/{actReq}" : $"既読: {readCount}/{total}";
        private static string GetReceivedStatusDisplay(bool isRead, bool reqAction) => !isRead ? "未読" : (reqAction ? "要対応" : "既読");
    }
}
