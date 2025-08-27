namespace keijiban.Models
{
    /// <summary>
    /// アプリケーションの設定ファイル(settings.json)に保存される内容を表すクラス。
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// ユーザーが最後に選択したスケジュールグループのID。
        /// </summary>
        public int? SelectedGroupId { get; set; }

        /// <summary>
        /// ユーザーが最後に選択した表示部署のID。
        /// </summary>
        public int? SelectedDepartmentId { get; set; }

        /// <summary>
        /// ユーザーが最後に選択した情報種別（スケジュール or 情報）のID。
        /// </summary>
        public int? SelectedInfoTypeId { get; set; }
    }
}
