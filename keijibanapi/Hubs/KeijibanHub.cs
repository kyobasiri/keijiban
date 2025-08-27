// ===============================
// Hubs/KeijibanHub.cs
// ===============================
using Microsoft.AspNetCore.SignalR;
using keijibanapi.Models;

namespace keijibanapi.Hubs
{
    public class KeijibanHub : Hub
    {
        private readonly ILogger<KeijibanHub> _logger;

        public KeijibanHub(ILogger<KeijibanHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // 緊急情報が更新されたことを全クライアントに通知
        public async Task NotifyEmergencyNoticeUpdate(List<EmergencyNotice> activeNotices)
        {
            await Clients.All.SendAsync("EmergencyNoticeUpdated", activeNotices);
        }

        // 特定部署向けの緊急情報更新通知
        public async Task NotifyEmergencyNoticeUpdateForDepartment(int departmentId, List<EmergencyNotice> activeNotices)
        {
            await Clients.All.SendAsync("EmergencyNoticeUpdatedForDepartment", new { DepartmentId = departmentId, Notices = activeNotices });
        }
    }
}
