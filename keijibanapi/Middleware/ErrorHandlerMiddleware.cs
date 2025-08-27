// ==========================================
// Middleware/ErrorHandlerMiddleware.cs
// ==========================================
using System.Net;
using System.Text.Json;

namespace keijibanapi.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        // 次に実行するミドルウェア(RequestDelegate)とロガーをDIで受け取る
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // ミドルウェアの本体となるメソッド
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // ここで次のミドルウェアやコントローラーのアクションが実行される
                await _next(context);
            }
            catch (Exception ex)
            {
                // _next(context)の実行中に、ハンドルされなかった例外が発生するとここでキャッチされる
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);

                // HTTPレスポンスを準備
                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 Internal Server Error

                // 全てのエラーで返す統一されたJSONレスポンスを作成
                var errorResponse = new
                {
                    success = false,
                    message = "サーバー内部で予期せぬエラーが発生しました。"
                    // 将来的にエラーIDなどを追加することも可能
                };

                var result = JsonSerializer.Serialize(errorResponse);
                await response.WriteAsync(result);
            }
        }
    }
}
