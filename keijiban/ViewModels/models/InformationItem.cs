using System;

namespace keijiban.ViewModels.Models
{
    /// <summary>
    /// インフォメーションパネルに表示されるお知らせの単一項目を表します。
    /// </summary>
    public class InformationItem
    {
        public DateTime PostDate { get; set; }
        public string DateDisplay { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsToday { get; set; }
    }
}
