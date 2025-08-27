using System;
using System.Collections.ObjectModel;

namespace keijiban.ViewModels.Models
{
    /// <summary>
    /// 週間スケジュールの1日分の列を表します。
    /// </summary>
    public class ScheduleColumn
    {
        public DateTime Date { get; set; }
        public bool IsToday { get; set; }
        public bool IsWeekend { get; set; }

        /// <summary>
        /// その日のすべてのスケジュール項目。
        /// </summary>
        public ObservableCollection<ScheduleItem> ScheduleItems { get; set; } = new();

        /// <summary>
        /// 画面に表示する、制限された数のスケジュール項目。
        /// </summary>
        public ObservableCollection<ScheduleItem> ScheduleItemsLimited { get; set; } = new();

        /// <summary>
        /// 表示しきれていない項目があるかどうかを示します。
        /// </summary>
        public bool HasMore { get; set; }
    }
}
