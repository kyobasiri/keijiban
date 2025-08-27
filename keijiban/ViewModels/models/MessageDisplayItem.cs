using keijiban.Models; // for MessagePriority

namespace keijiban.ViewModels.Models
{
    /// <summary>
    /// 部署間連絡パネルに表示されるメッセージの単一項目を表します。
    /// </summary>
    public class MessageDisplayItem
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string FromDeptName { get; set; } = string.Empty;
        public string ToDeptName { get; set; } = string.Empty;
        public MessagePriority Priority { get; set; }
        public string PriorityIcon { get; set; } = string.Empty;
        public string DateDisplay { get; set; } = string.Empty;
        public string DueDateDisplay { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public bool IsRead { get; set; }

        /// <summary>
        /// 締め切り日が設定されているかどうかを判定します。UIの表示/非表示に使います。
        /// </summary>
        public bool HasDueDate => !string.IsNullOrEmpty(DueDateDisplay);
    }
}
