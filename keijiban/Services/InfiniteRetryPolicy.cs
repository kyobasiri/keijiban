using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace keijiban.Services
{
    /// <summary>
    /// SignalRの接続が切れた際に、無制限に再接続を試行するカスタムポリシー。
    /// 再試行の間隔は、試行回数に応じて段階的に長くなります（Exponential Backoff）。
    /// </summary>
    public class InfiniteRetryPolicy : IRetryPolicy
    {
        private readonly Random _random = new();

        /// <summary>
        /// 次の再接続試行までの遅延時間を計算します。
        /// </summary>
        /// <param name="context">現在の再試行に関するコンテキスト情報。</param>
        /// <returns>次の再試行までの待機時間。TimeSpan?</returns>
        public TimeSpan? NextRetryDelay(RetryContext context)
        {
            // context.PreviousRetryCount は0から始まります。
            switch (context.PreviousRetryCount)
            {
                // 初回は即時再試行
                case 0: return TimeSpan.FromSeconds(0);
                // 2回目は2秒後
                case 1: return TimeSpan.FromSeconds(2);
                // 3回目は10秒後
                case 2: return TimeSpan.FromSeconds(10);
                // 4回目は30秒後
                case 3: return TimeSpan.FromSeconds(30);
                // 5回目以降はずっと約1分間隔
                default:
                    // 複数のクライアントが同時に再接続を試みてサーバーに負荷をかける
                    //「Thundering Herd」問題を避けるため、基本の待機時間にランダムな「ゆらぎ」を追加します。
                    var randomJitterInSeconds = _random.Next(-5, 6); // -5秒から+5秒の範囲
                    return TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(randomJitterInSeconds));
            }
        }
    }
}
