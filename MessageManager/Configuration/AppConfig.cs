using Microsoft.Extensions.Configuration;

namespace MessageManager.Configuration
{
    public static class AppConfig
    {
        public static string ApiBaseUrl { get; private set; } = "";
        public static string SignalRUrl { get; private set; } = "";
        public static int ApiTimeout { get; private set; } = 30;
        public static string Environment { get; private set; } = "";

        /// <summary>
        /// appsettings.jsonから設定値を一度だけ読み込む
        /// </summary>
        public static void Initialize(IConfiguration configuration)
        {
            ApiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7052/api";
            SignalRUrl = configuration.GetValue<string>("ApiSettings:SignalRUrl") ?? "https://localhost:7052/keijibanHub";
            ApiTimeout = configuration.GetValue<int>("ApiSettings:Timeout", 30);
            Environment = configuration.GetValue<string>("Environment") ?? "Unknown";
        }

        public static string GetConfigSummary()
        {
            return $"Environment: {Environment}, ApiBaseUrl: {ApiBaseUrl}, SignalRUrl: {SignalRUrl}";
        }
    }
}
