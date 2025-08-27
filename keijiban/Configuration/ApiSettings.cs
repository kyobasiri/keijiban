namespace keijiban.Configuration
{
    /// <summary>
    /// appsettings.jsonの "ApiSettings" セクションをマッピングするためのクラス。
    /// </summary>
    public class ApiSettings
    {
        public const string SectionName = "ApiSettings";

        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30;
        public string SignalRUrl { get; set; } = string.Empty;
    }
}
