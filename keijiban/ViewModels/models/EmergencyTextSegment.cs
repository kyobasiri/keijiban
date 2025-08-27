namespace keijiban.ViewModels.Models
{
    /// <summary>
    /// 緊急情報マーキー表示で、色分けされたテキストの断片を表します。
    /// </summary>
    public class EmergencyTextSegment
    {
        public string Text { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
    }
}
