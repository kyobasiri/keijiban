namespace keijiban.ViewModels.Models
{
    /// <summary>
    /// スケジュール列に表示されるスケジュールの単一項目を表します。
    /// </summary>
    public class ScheduleItem
    {
        public string Time { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // 部署名やカテゴリなど
    }
}
