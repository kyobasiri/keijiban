// =======================================
// Services/IScheduleService.cs - 最終形
// =======================================
using keijibanapi.Models;

namespace keijibanapi.Services
{
    public interface IScheduleService
    {
        Task<ScheduleResponse> GetScheduleGroupDataAsync(DateTime startDate, int? departmentId = null);
        Task<ScheduleResponse> GetDepartmentScheduleDataAsync(DateTime startDate, int? departmentId = null);
        Task<InformationApiResponse> GetInformationAsync(int? departmentId = null);

        Task<ScheduleResponse> GetCombinedScheduleGroupDataAsync(DateTime startDate, int? departmentId = null);
        // ★ GetCombinedScheduleGroupDataAsync はコントローラーの責務分離で追加したメソッド
        // (前回の解説で追加済み)
    }

    // IDoctorAbsenceService はこのファイルから分離しても良いですが、そのままでも動作します
    public interface IDoctorAbsenceService
    {
        Task<DoctorAbsenceResponse> GetDoctorAbsencesAsync(DateTime startDate);
    }
}
