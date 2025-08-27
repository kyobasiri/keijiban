using keijiban.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace keijiban.Services
{
    /// <summary>
    /// SignalRハブとのリアルタイム通信を管理するサービスのインターフェース。
    /// </summary>
    public interface ISignalRService : IAsyncDisposable
    {
        /// <summary>
        /// 緊急情報がサーバーからプッシュ更新されたときに発生するイベント。
        /// </summary>
        event Action<List<EmergencyNoticeApiItem>>? EmergencyNoticeUpdated;

        /// <summary>
        /// SignalRハブへの接続を開始します。
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// SignalRハブへの接続を停止します。
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 現在SignalRハブに接続されているかどうかを示す値を取得します。
        /// </summary>
        bool IsConnected { get; }
    }
}
