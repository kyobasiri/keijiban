// ===============================
// MessageManager/Models/MessageModels.cs - 返信済み状態対応版
// ===============================
using MessageManager.Services;
using System;
using System.Collections.Generic;

namespace MessageManager.Models
{
    public class Department
    {
        public int? Id { get; set; }
        public string? Name { get; set; } = "";
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

    // 表示用モデル
    public class MessageListItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string FromDeptName { get; set; } = "";
        public Priority Priority { get; set; }
        public string PriorityDisplay { get; set; } = "";
        public string PriorityIcon { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string DateDisplay { get; set; } = "";
        public DateTimeOffset? DueDate { get; set; }
        public string DueDateDisplay { get; set; } = "";
        public bool IsRead { get; set; }
        public bool RequiresAction { get; set; }
        public string StatusDisplay { get; set; } = "";
        public string StatusColor { get; set; } = "";
        public bool HasDueDate => !string.IsNullOrEmpty(DueDateDisplay);
        public bool IsDone { get; set; } = false;

        public ActionStatus ActionStatus { get; set; } = ActionStatus.Pending;
        public string ActionStatusDisplay => GetActionStatusDisplay(ActionStatus);
        public string ActionStatusColor => GetActionStatusColor(ActionStatus);

        private static string GetActionStatusDisplay(ActionStatus status) => status switch
        {
            ActionStatus.Pending => "未対応",
            ActionStatus.InProgress => "対応中",
            ActionStatus.Completed => "完了",
            ActionStatus.Rejected => "却下",
            ActionStatus.Replied => "返信済み", // ★ 新規追加
            _ => "未対応"
        };

        private static string GetActionStatusColor(ActionStatus status) => status switch
        {
            ActionStatus.Pending => "#6c757d",
            ActionStatus.InProgress => "#ffc107",
            ActionStatus.Completed => "#28a745",
            ActionStatus.Rejected => "#dc3545",
            ActionStatus.Replied => "#17a2b8", // ★ 新規追加: 青色
            _ => "#6c757d"
        };
    }

    public class SentMessageListItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string ToDeptNames { get; set; } = "";
        public Priority Priority { get; set; }
        public string PriorityDisplay { get; set; } = "";
        public string PriorityIcon { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string DateDisplay { get; set; } = "";
        public DateTimeOffset? DueDate { get; set; }
        public string DueDateDisplay { get; set; } = "";
        public bool RequiresAction { get; set; }
        public bool HasDueDate => !string.IsNullOrEmpty(DueDateDisplay);
        public bool IsDone { get; set; } = false;
    }

    public class MessageDetailItem
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
        public DateTimeOffset? DueDate { get; set; }
        public string DueDateDisplay { get; set; } = "";
        public bool RequiresAction { get; set; }
        public string PriorityIcon { get; set; } = "";
        public bool HasDueDate => !string.IsNullOrEmpty(DueDateDisplay);
        public bool IsDone { get; set; } = false;
    }

    public class MessageRecipientItem
    {
        public string DeptName { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string ReadDisplay { get; set; } = "";
    }

    public class MessageActionItem
    {
        public string DeptName { get; set; } = "";
        public ActionStatus Status { get; set; }
        public string StatusDisplay { get; set; } = "";
        public string Comment { get; set; } = "";
        public DateTime? ActionDate { get; set; }
        public string ActionDisplay { get; set; } = "";

        public string StatusColor => GetActionStatusColor(Status);

        private static string GetActionStatusColor(ActionStatus status) => status switch
        {
            ActionStatus.Pending => "#6c757d",
            ActionStatus.InProgress => "#ffc107",
            ActionStatus.Completed => "#28a745",
            ActionStatus.Rejected => "#dc3545",
            ActionStatus.Replied => "#17a2b8", // ★ 新規追加: 青色
            _ => "#6c757d"
        };
    }

    // API用モデル
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

    public class ActionUpdateRequest
    {
        public int MessageId { get; set; }
        public ActionStatus Status { get; set; }
        public string Comment { get; set; } = "";
    }

    public class MessageDoneRequest
    {
        public int MessageId { get; set; }
        public bool IsDone { get; set; }
    }

    // APIレスポンス用モデル
    public class MessageListResponse : IApiResponse
    {
        public List<MessageListApiItem> Messages { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class SentMessageListResponse : IApiResponse
    {
        public List<SentMessageListApiItem> Messages { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class MessageDetailResponse : IApiResponse
    {
        public MessageDetailApiItem? MessageDetail { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class SendMessageResponse : IApiResponse
    {
        public int MessageId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class ActionUpdateResponse : IApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class MessageDoneResponse : IApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class DepartmentApiResponse : IApiResponse
    {
        public List<Department> Departments { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    // API用詳細クラス
    public class MessageListApiItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string FromDeptName { get; set; } = "";
        public Priority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public bool IsRead { get; set; }
        public bool RequiresAction { get; set; }
        public string StatusDisplay { get; set; } = "";
        public string StatusColor { get; set; } = "";
        public ActionStatus? ActionStatus { get; set; }
        public bool IsDone { get; set; } = false;
    }

    public class SentMessageListApiItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string ToDeptNames { get; set; } = "";
        public Priority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public bool RequiresAction { get; set; }
        public bool IsDone { get; set; } = false;
    }

    public class MessageDetailApiItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
        public string FromDeptName { get; set; } = "";
        public Priority Priority { get; set; }
        public string Category { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public bool RequiresAction { get; set; }
        public bool IsDone { get; set; } = false;
        public List<MessageRecipientApiItem> Recipients { get; set; } = new();
        public List<MessageActionApiItem> Actions { get; set; } = new();
    }

    public class MessageRecipientApiItem
    {
        public string ToDeptName { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class MessageActionApiItem
    {
        public string DeptName { get; set; } = "";
        public ActionStatus ActionStatus { get; set; }
        public string ActionComment { get; set; } = "";
        public DateTime? ActionDate { get; set; }
    }

}
