// Services/IApiResponse.cs

namespace MessageManager.Services
{
    /// <summary>
    /// APIレスポンスが共通して持つプロパティを定義するインターフェース。
    /// </summary>
    public interface IApiResponse
    {
        bool Success { get; set; }
        string Message { get; set; }
    }
}
