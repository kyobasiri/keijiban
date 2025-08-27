// ==========================================
// Services/DoctorAbsenceService.cs - リファクタリング後
// ==========================================
using keijibanapi.Models;
using keijibanapi.Repositories;

namespace keijibanapi.Services
{
    public class DoctorAbsenceService : IDoctorAbsenceService
    {
        private readonly IDoctorAbsenceRepository _doctorAbsenceRepository;
        private readonly ILogger<DoctorAbsenceService> _logger;

        public DoctorAbsenceService(IDoctorAbsenceRepository doctorAbsenceRepository, ILogger<DoctorAbsenceService> logger)
        {
            _doctorAbsenceRepository = doctorAbsenceRepository;
            _logger = logger;
        }

        public async Task<DoctorAbsenceResponse> GetDoctorAbsencesAsync(DateTime startDate)
        {
            try
            {
                var doctorAbsences = await _doctorAbsenceRepository.GetDoctorAbsencesAsync(startDate);

                // 日付ごとのグループ化はビジネスロジックなので、サービス層に残す
                var doctorSchedules = doctorAbsences
                    .GroupBy(absence => absence.Date.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.ToList());

                _logger.LogInformation($"Retrieved {doctorAbsences.Count()} doctor absence records, grouped into {doctorSchedules.Count} dates");

                return new DoctorAbsenceResponse
                {
                    DoctorSchedules = doctorSchedules,
                    Success = true,
                    Message = "医師不在予定データを正常に取得しました"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor absence data");
                return new DoctorAbsenceResponse { Success = false, Message = $"医師不在予定データの取得に失敗しました: {ex.Message}" };
            }
        }
    }
}
