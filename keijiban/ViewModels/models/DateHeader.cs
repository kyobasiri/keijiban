using System;

namespace keijiban.ViewModels.Models
{
    /// <summary>
    /// 週間スケジュールのヘッダーに表示される日付情報を表します。
    /// </summary>
    public class DateHeader
    {
        public DateTime Date { get; set; }
        public string Header { get; set; } = string.Empty;
        public bool IsToday { get; set; }
        public bool IsWeekend { get; set; }
    }
}
