namespace MessageManager.Configuration
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30;
        public string SignalRUrl { get; set; } = string.Empty;
    }
}
