using keijiban.Configuration;
using keijiban.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace keijiban.Services
{
    /// <summary>
    /// SignalRハブとのリアルタイム通信を管理するサービスの実装。
    /// </summary>
    public class SignalRService : ISignalRService
    {
        private readonly string _hubUrl;
        private readonly ILogger<SignalRService> _logger;
        private HubConnection? _hubConnection;

        public event Action<List<EmergencyNoticeApiItem>>? EmergencyNoticeUpdated;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public SignalRService(IOptions<ApiSettings> apiSettingsOptions, ILogger<SignalRService> logger)
        {
            _logger = logger;
            _hubUrl = apiSettingsOptions.Value.SignalRUrl;
            _logger.LogInformation("SignalRService initialized with Hub URL: {HubUrl}", _hubUrl);
        }

        public async Task StartAsync()
        {
            if (_hubConnection != null)
            {
                _logger.LogWarning("SignalR connection already exists. Cannot start a new one.");
                return;
            }

            _logger.LogInformation("Starting SignalR connection to {HubUrl}...", _hubUrl);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect(new InfiniteRetryPolicy())
                .Build();

            // サーバーから "EmergencyNoticeUpdated" メソッドが呼び出されたときの処理を登録
            _hubConnection.On<List<EmergencyNoticeApiItem>>("EmergencyNoticeUpdated", (emergencyNotices) =>
            {
                _logger.LogInformation("Received EmergencyNoticeUpdated event with {Count} items.", emergencyNotices.Count);
                EmergencyNoticeUpdated?.Invoke(emergencyNotices);
            });

            // 接続状態に関するイベントのロギング
            _hubConnection.Reconnecting += (error) =>
            {
                _logger.LogWarning(error, "SignalR connection is reconnecting...");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                _logger.LogInformation("SignalR connection reconnected with ID: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += (error) =>
            {
                // `StopAsync`による意図的な切断の場合、errorはnullになる
                if (error != null)
                {
                    _logger.LogError(error, "SignalR connection was closed due to an error.");
                }
                else
                {
                    _logger.LogInformation("SignalR connection closed gracefully.");
                }
                return Task.CompletedTask;
            };

            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("SignalR connected successfully. Connection ID: {ConnectionId}", _hubConnection.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "SignalR initial connection failed to {HubUrl}", _hubUrl);
                // 必要であれば、ここでリトライ処理やUIへの通知を行う
            }
        }

        public async Task StopAsync()
        {
            if (_hubConnection != null)
            {
                _logger.LogInformation("Stopping SignalR connection...");
                await _hubConnection.StopAsync();
                // DisposeAsyncでリソース解放を行うため、ここではStopAsyncのみ呼び出す
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                _logger.LogDebug("Disposing SignalR connection resources.");
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
            // Disposeパターンに従い、GCでのファイナライザ呼び出しを抑制
            GC.SuppressFinalize(this);
        }
    }
}
