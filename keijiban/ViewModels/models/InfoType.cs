namespace keijiban.ViewModels.Models
{
    /// <summary>
    /// フッターで選択可能な情報種別（スケジュール、情報など）を表します。
    /// </summary>
    public class InfoType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
