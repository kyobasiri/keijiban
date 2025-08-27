// ==========================================
// Services/ExtraScheduleService.cs - リファクタリング後
// ==========================================
using keijibanapi.Models;
using keijibanapi.Repositories;

namespace keijibanapi.Services
{
    public class ExtraScheduleService : IExtraScheduleService
    {
        private readonly IExtraScheduleRepository _extraScheduleRepository;
        private readonly ILogger<ExtraScheduleService> _logger;

        public ExtraScheduleService(IExtraScheduleRepository extraScheduleRepository, ILogger<ExtraScheduleService> logger)
        {
            _extraScheduleRepository = extraScheduleRepository;
            _logger = logger;
        }

        public async Task<ExtraScheduleDataResponse> GetExtraScheduleDataAsync(int? departmentId = null, DateTime? startDate = null)
        {
            try
            {
                var targetStartDate = startDate ?? DateTime.Today;
                var targetEndDate = targetStartDate.AddDays(7);
                var extraSchedules = await _extraScheduleRepository.GetExtraScheduleDataAsync(targetStartDate, targetEndDate, departmentId);

                return new ExtraScheduleDataResponse
                {
                    ExtraSchedules = extraSchedules.ToList(),
                    Success = true,
                    Message = "その他スケジュールデータを正常に取得しました"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving extra schedule data for department {DepartmentId}", departmentId);
                return new ExtraScheduleDataResponse { Success = false, Message = $"その他スケジュールデータの取得に失敗しました: {ex.Message}" };
            }
        }
    }
}
