namespace keijiban.Configuration
{
    /// <summary>
    /// アプリケーション全体で使用される固定値（定数）を定義します。
    /// これらの値は、プログラムのロジックの一部として機能し、通常はビルド後に変更されません。
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// デフォルト設定に関する定数を定義します。
        /// </summary>
        public static class Defaults
        {
            /// <summary>
            /// 設定ファイルが存在しない場合に選択される、デフォルトのスケジュールグループID。
            /// (例: 100 = 清和会)
            /// </summary>
            public const int ScheduleGroupId = 100;

            /// <summary>
            /// 設定ファイルが存在しない場合に選択される、デフォルトの情報種別ID。
            /// (例: 1 = スケジュール)
            /// </summary>
            public const int InfoTypeId = 1;
        }

        /// <summary>
        /// 表示される情報種別を定義します。
        /// </summary>
        public static class InfoTypes
        {
            public const int Schedule = 1;
            public const int Information = 2;
        }

        /// <summary>
        /// UIで使用される色に関する定数を定義します。
        /// XAMLのResourceDictionaryで管理するのが理想ですが、C#側で動的に色を扱う場合などに使用します。
        /// </summary>
        public static class Colors
        {
            public const string DefaultHeaderBackground = "White";
            public const string EmergencyHeaderBackground = "#FFD700"; // 黄色
            public const string DefaultText = "#000000"; // 黒
            public const string UrgentPriorityText = "#DC143C"; // 赤 (クリムゾン)
            public const string HighPriorityText = "#228B22";   // 緑 (フォレストグリーン)
        }

        /// <summary>
        /// 著作権情報を定義します。
        /// </summary>
        public const string Copyright = "© 2025 病院内掲示板システム";
    }
}
