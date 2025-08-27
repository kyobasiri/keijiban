// ===============================
// Controllers/EmergencyNoticeController.cs - 複数部署対応版
// ===============================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using keijibanapi.Models;
using keijibanapi.Services;
using keijibanapi.Hubs;

namespace keijibanapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyNoticeController : ControllerBase
    {
        private readonly IEmergencyNoticeService _emergencyNoticeService;
        private readonly IHubContext<KeijibanHub> _hubContext;
        private readonly ILogger<EmergencyNoticeController> _logger;

        public EmergencyNoticeController(
            IEmergencyNoticeService emergencyNoticeService,
            IHubContext<KeijibanHub> hubContext,
            ILogger<EmergencyNoticeController> logger)
        {
            _emergencyNoticeService = emergencyNoticeService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// 全ての緊急連絡事項を取得
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<GetEmergencyNoticesResponse>> GetAllNotices()
        {
                _logger.LogInformation("Getting all emergency notices");

                var result = await _emergencyNoticeService.GetAllNoticesAsync();

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// ★新規追加：複数部署向けの有効な緊急連絡事項を統合取得
        /// スケジュールグループ選択部署 + 表示部署 + 全部署向けの連絡事項を統合
        /// </summary>
        [HttpGet("departments/combined/active")]
        public async Task<ActionResult<GetActiveNoticesForDepartmentResponse>> GetCombinedActiveNoticesForDepartments(
            [FromQuery] int? scheduleGroupDepartmentId = null,
            [FromQuery] int? displayDepartmentId = null)
        {
                _logger.LogInformation($"Getting combined active emergency notices for schedule group: {scheduleGroupDepartmentId}, display dept: {displayDepartmentId}");

                var allNotices = new List<EmergencyNotice>();
                var tasks = new List<Task<GetActiveNoticesForDepartmentResponse>>();

                // 1. スケジュールグループ部署の緊急連絡事項
                if (scheduleGroupDepartmentId.HasValue && scheduleGroupDepartmentId.Value > 0)
                {
                    tasks.Add(_emergencyNoticeService.GetActiveNoticesForDepartmentAsync(scheduleGroupDepartmentId.Value));
                }

                // 2. 表示部署の緊急連絡事項（スケジュールグループと異なる場合のみ）
                if (displayDepartmentId.HasValue && displayDepartmentId.Value > 0 &&
                    displayDepartmentId.Value != scheduleGroupDepartmentId)
                {
                    tasks.Add(_emergencyNoticeService.GetActiveNoticesForDepartmentAsync(displayDepartmentId.Value));
                }

                // 全部署対象の緊急連絡事項は各部署別取得で自動的に含まれる

                var results = await Task.WhenAll(tasks);

                // 結果をマージ（重複除去 + 有効なもののみ）
                var noticeDict = new Dictionary<int, EmergencyNotice>();

                foreach (var result in results)
                {
                    if (result.Success)
                    {
                        foreach (var notice in result.Notices)
                        {
                            // ★修正：有効かつ重複していないもののみ追加
                            if (notice.IsActive && !noticeDict.ContainsKey(notice.Id))
                            {
                                noticeDict[notice.Id] = notice;
                            }
                        }
                    }
                }

                var combinedNotices = noticeDict.Values
                    .OrderByDescending(n => n.Priority)  // 優先度順
                    .ThenByDescending(n => n.CreatedAt)   // 作成日時順
                    .ToList();

                // 連絡事項を文字列連結
                var combinedContent = string.Join(" | ", combinedNotices.Select(n => $"【{n.NoticeType}】{n.NoticeContent}"));

                _logger.LogInformation($"Retrieved {combinedNotices.Count} combined emergency notices");

                return Ok(new GetActiveNoticesForDepartmentResponse
                {
                    Notices = combinedNotices,
                    CombinedContent = combinedContent,
                    Success = true,
                    Message = "統合緊急連絡事項を正常に取得しました"
                });
        }

        /// <summary>
        /// 指定部署の有効な緊急連絡事項を取得（従来版）
        /// </summary>
        [HttpGet("department/{departmentId}/active")]
        public async Task<ActionResult<GetActiveNoticesForDepartmentResponse>> GetActiveNoticesForDepartment(
            int departmentId)
        {
                _logger.LogInformation($"Getting active emergency notices for department: {departmentId}");

                var result = await _emergencyNoticeService.GetActiveNoticesForDepartmentAsync(departmentId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        [HttpPost]
        public async Task<ActionResult<EmergencyNoticeResponse>> CreateNotice(
            [FromBody] CreateEmergencyNoticeRequest request)
        {
                _logger.LogInformation($"Creating emergency notice: {request.NoticeType}");

                var result = await _emergencyNoticeService.CreateNoticeAsync(request);

                if (result.Success)
                {
                    await NotifyClientsOfEmergencyUpdate();
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EmergencyNoticeResponse>> UpdateNotice(
            int id,
            [FromBody] UpdateEmergencyNoticeRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new EmergencyNoticeResponse
                {
                    Success = false,
                    Message = "IDが一致しません"
                });
            }

                _logger.LogInformation($"Updating emergency notice ID: {id}");

                var result = await _emergencyNoticeService.UpdateNoticeAsync(request);

                if (result.Success)
                {
                    await NotifyClientsOfEmergencyUpdate();
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<EmergencyNoticeResponse>> ToggleNotice(
            int id,
            [FromBody] ToggleEmergencyNoticeRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new EmergencyNoticeResponse
                {
                    Success = false,
                    Message = "IDが一致しません"
                });
            }

                _logger.LogInformation($"Toggling emergency notice ID: {id} to {(request.IsActive ? "active" : "inactive")}");

                var result = await _emergencyNoticeService.ToggleNoticeAsync(request);

                if (result.Success)
                {
                    await NotifyClientsOfEmergencyUpdate();
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<EmergencyNoticeResponse>> DeleteNotice(int id)
        {
                _logger.LogInformation($"Deleting emergency notice ID: {id}");

                var result = await _emergencyNoticeService.DeleteNoticeAsync(id);

                if (result.Success)
                {
                    await NotifyClientsOfEmergencyUpdate();
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        private async Task NotifyClientsOfEmergencyUpdate()
        {
            try
            {
                var allNotices = await _emergencyNoticeService.GetAllNoticesAsync();
                if (allNotices.Success)
                {
                    var activeNotices = allNotices.Notices.Where(n => n.IsActive).ToList();
                    await _hubContext.Clients.All.SendAsync("EmergencyNoticeUpdated", activeNotices);
                    _logger.LogInformation($"Sent EmergencyNoticeUpdated to all clients with {activeNotices.Count} notices");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification");
            }
        }
    }
}
