// ===============================
// Controllers/ScheduleController.cs - 部署メンテナンス機能追加版
// ===============================
using Microsoft.AspNetCore.Mvc;
using keijibanapi.Models;
using keijibanapi.Services;

namespace keijibanapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly IDoctorAbsenceService _doctorAbsenceService;
        private readonly IExtraScheduleService _extraScheduleService;
        private readonly ILogger<ScheduleController> _logger;
        private readonly IDepartmentService _departmentService;

        public ScheduleController(
            IScheduleService scheduleService,
            IDoctorAbsenceService doctorAbsenceService,
            IExtraScheduleService extraScheduleService,
            ILogger<ScheduleController> logger,
            IDepartmentService departmentService)
        {
            _scheduleService = scheduleService;
            _doctorAbsenceService = doctorAbsenceService;
            _extraScheduleService = extraScheduleService;
            _logger = logger;
            _departmentService = departmentService;
        }

        /// <summary>
        /// スケジュールグループデータ取得（1行目用）- 選択グループ + 清和会統合版
        /// </summary>
        [HttpGet("group")]
        public async Task<ActionResult<ScheduleResponse>> GetScheduleGroupData(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] int? departmentId = null)
        {
            var date = startDate ?? DateTime.Today;
            _logger.LogInformation($"Getting combined schedule group data for department {departmentId} + 清和会(100) from {date}");

            // ★ビジネスロジックは全てこの一行に集約された！
            var result = await _scheduleService.GetCombinedScheduleGroupDataAsync(date, departmentId);

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
        /// 部署スケジュールデータ取得（2行目用）- 従来通り
        /// </summary>
        [HttpGet("department")]
        public async Task<ActionResult<ScheduleResponse>> GetDepartmentScheduleData(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] int? departmentId = null)
        {
            var date = startDate ?? DateTime.Today;
            _logger.LogInformation($"Getting department schedule data from {date} for department {departmentId}");

            var result = await _scheduleService.GetDepartmentScheduleDataAsync(date, departmentId);

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
        /// その他スケジュールデータ取得（②情報選択時用）
        /// </summary>
        [HttpGet("extra-data")]
        public async Task<ActionResult<ExtraScheduleDataResponse>> GetExtraScheduleData(
            [FromQuery] int? departmentId = null,
            [FromQuery] DateTime? startDate = null)
        {
            var date = startDate ?? DateTime.Today;
            _logger.LogInformation($"Getting extra schedule data from {date} for department {departmentId}");

            var result = await _extraScheduleService.GetExtraScheduleDataAsync(departmentId, date);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("doctor-absences")]
        public async Task<ActionResult<DoctorAbsenceResponse>> GetDoctorAbsences(
            [FromQuery] DateTime? startDate = null)
        {
            var date = startDate ?? DateTime.Today;
            _logger.LogInformation($"Getting doctor absence data from {date}");

            var result = await _doctorAbsenceService.GetDoctorAbsencesAsync(date);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("departments")]
        public async Task<ActionResult<DepartmentApiResponse>> GetDepartments(
            [FromQuery] int? displaycase = null)
        {
            // ▼▼▼▼▼ このブロックを一時的に追加 ▼▼▼▼▼
            _logger.LogInformation("--- Start Request Headers for /departments ---");
            foreach (var header in Request.Headers)
            {
                _logger.LogInformation($"Header: {header.Key} = {header.Value}");
            }
            _logger.LogInformation("--- End Request Headers for /departments ---");
            // ▲▲▲▲▲ ここまで ▲▲▲▲▲
            _logger.LogInformation("Getting departments list from active system");

            var departments = await _departmentService.GetActiveDepartmentsAsync(displaycase);

            var result = new DepartmentApiResponse
            {
                Departments = departments,
                Success = true,
                Message = "部署リストを正常に取得しました"
            };

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("lists")]
        public async Task<IActionResult> GetDepartmentLists()
        {
            var result = await _departmentService.GetDepartmentListsAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }

        /// <summary>
        /// 全部署マスタ取得（メンテナンス用）
        /// </summary>
        [HttpGet("department-masters")]
        public async Task<ActionResult<DepartmentMasterListResponse>> GetAllDepartmentMasters()
        {
            _logger.LogInformation("Getting all department masters for maintenance");

            // ★ 呼び出し先を _departmentService に変更
            var result = await _departmentService.GetAllDepartmentMastersAsync();

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
        /// 部署マスタ更新
        /// </summary>
        [HttpPut("department-master")]
        public async Task<ActionResult<UpdateDepartmentMasterResponse>> UpdateDepartmentMaster(
            [FromBody] UpdateDepartmentMasterRequest request)
        {
            _logger.LogInformation($"Updating department master ID: {request.Id}");

            // ★ 呼び出し先を _departmentService に変更
            var result = await _departmentService.UpdateDepartmentMasterAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }



        [HttpGet("information")]
        public async Task<ActionResult<InformationApiResponse>> GetInformation(
            [FromQuery] int? departmentId = null)
        {
            _logger.LogInformation($"Getting information data for department {departmentId}");

            var result = await _scheduleService.GetInformationAsync(departmentId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.Now });
        }
    }
}
