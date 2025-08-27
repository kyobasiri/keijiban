// ===============================
// Models/MessageModels.cs (API側) - 返信済み対応版
// ===============================
namespace keijibanapi.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int FromDeptId { get; set; }
        public string FromDeptName { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
        public Priority Priority { get; set; } = Priority.Normal;
        public string Category { get; set; } = "";
        public DateTime? DueDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool RequiresAction { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int MessageDone { get; set; } = 0;
        public int RecipientCount { get; set; }
        public int ReadCount { get; set; }
        public int ActionRequiredCount { get; set; }
        public int ActionCompletedCount { get; set; }
        public int? OriginalMessageId { get; set; } // ★ 新規追加: 返信元メッセージID

        // 受信者情報（複数部署宛ての場合）
        public List<MessageRecipient> Recipients { get; set; } = new();
        public List<MessageAction> Actions { get; set; } = new();
    }

    public class MessageRecipient
    {
        public int RecipientId { get; set; }
        public int MessageId { get; set; }
        public int ToDeptId { get; set; }
        public string ToDeptName { get; set; } = "";
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public bool RequiresAction { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }

    public class MessageAction
    {
        public int ActionId { get; set; }
        public int MessageId { get; set; }
        public int DeptId { get; set; }
        public string DeptName { get; set; } = "";
        public ActionStatus ActionStatus { get; set; } = ActionStatus.Pending;
        public string ActionComment { get; set; } = "";
        public DateTime? ActionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum Priority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public enum ActionStatus
    {
        Pending,
        InProgress,
        Completed,
        Rejected,
        Replied // ★ 新規追加: 返信済み状態
    }

    // API用のレスポンスクラス
    public class MessageListResponse
    {
        public List<MessageListItem> Messages { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class MessageDetailResponse
    {
        public MessageDetail? MessageDetail { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class SendMessageRequest
    {
        public List<int> ToDeptIds { get; set; } = new();
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
        public Priority Priority { get; set; } = Priority.Normal;
        public string Category { get; set; } = "";
        public DateTime? DueDate { get; set; }
        public bool RequiresAction { get; set; } = false;
    }

    public class SendMessageResponse
    {
        public int MessageId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class MessageDoneRequest
    {
        public int MessageId { get; set; }
        public bool IsDone { get; set; }
    }

    public class MessageDoneResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    // 表示用のクラス
    public class MessageListItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string FromDeptName { get; set; } = "";
        public string ToDeptNames { get; set; } = "";
        public Priority Priority { get; set; }
        public string PriorityDisplay { get; set; } = "";
        public string PriorityIcon { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string DateDisplay { get; set; } = "";
        public DateTime? DueDate { get; set; }
        public string DueDateDisplay { get; set; } = "";
        public bool IsRead { get; set; }
        public bool RequiresAction { get; set; }
        public string StatusDisplay { get; set; } = "";
        public string StatusColor { get; set; } = "";
        public int ReadCount { get; set; }
        public int TotalRecipients { get; set; }
        public string ReadProgress { get; set; } = "";
        public ActionStatus? ActionStatus { get; set; }
        public bool IsDone { get; set; } = false;
        public int? OriginalMessageId { get; set; } // ★ 新規追加: 返信元メッセージID
    }

    public class MessageDetail
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
        public string FromDeptName { get; set; } = "";
        public Priority Priority { get; set; }
        public string PriorityDisplay { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string DateDisplay { get; set; } = "";
        public DateTime? DueDate { get; set; }
        public string DueDateDisplay { get; set; } = "";
        public bool RequiresAction { get; set; }
        public bool IsDone { get; set; } = false;
        public int? OriginalMessageId { get; set; } // ★ 新規追加: 返信元メッセージID
        public List<MessageRecipient> Recipients { get; set; } = new();
        public List<MessageAction> Actions { get; set; } = new();
    }

    public class ActionUpdateRequest
    {
        public int MessageId { get; set; }
        public ActionStatus Status { get; set; }
        public string Comment { get; set; } = "";
    }

    public class ActionUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    // APIレスポンス用のクラス
    public class SentMessageListResponse
    {
        public List<SentMessageListApiItem> Messages { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    // 送信メッセージ一覧のAPI用アイテム
    public class SentMessageListApiItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string ToDeptNames { get; set; } = "";
        public Priority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool RequiresAction { get; set; }
        public bool IsDone { get; set; } = false;
        public int? OriginalMessageId { get; set; } // ★ 新規追加: 返信元メッセージID
    }
}
