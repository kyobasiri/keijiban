using System;

namespace keijiban.Configuration
{
    /// <summary>
    /// アプリケーションの外部設定値を管理します。
    /// これらの値は、通常アプリケーションのビルド後に環境ごとに変更される可能性があります。
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// 接続するバックエンドAPIのベースURL。
        /// 例: "http://localhost:5000/api"
        /// </summary>
        public static string ApiBaseUrl { get; } = "https://localhost:7052/api"; // ★ご自身の環境に合わせて変更してください        /// <summary>        /// APIリクエストのタイムアウト時間（秒）。
        /// </summary>
        public static int ApiTimeout { get; } = 30;

        /// <summary>
        /// 接続するSignalRハブのURL。
        /// </summary>
        public static string SignalRUrl { get; } = "https://localhost:7052/keijibanhub"; // ★ご自身の環境に合わせて変更してください
    }
}
