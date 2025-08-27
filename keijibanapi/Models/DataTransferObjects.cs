// ===================================
// Models/DataTransferObjects.cs
// ===================================
using keijibanapi.Models; // PriorityやActionStatusを参照するため

namespace keijibanapi.DataTransferObjects
{
    /// <summary>
    /// リポジトリが受信メッセージ一覧を取得する際に使用する専用のデータ転送オブジェクト
    /// </summary>
    public class ReceivedMessageDto
    {
        public int MessageId { get; set; }
        public string Subject { get; set; } = "";
        public int FromDeptId { get; set; }
        public Priority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public bool RequiresAction { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MessageDone { get; set; }
        public int? OriginalMessageId { get; set; }
        public bool IsRead { get; set; }
        public ActionStatus? ActionStatus { get; set; }
    }
}
