// =======================================
// Services/IEmergencyNoticeService.cs
// =======================================
using keijibanapi.Models;

namespace keijibanapi.Services
{
    public interface IEmergencyNoticeService
    {
        Task<GetEmergencyNoticesResponse> GetAllNoticesAsync();
        Task<EmergencyNoticeResponse> CreateNoticeAsync(CreateEmergencyNoticeRequest request);
        Task<EmergencyNoticeResponse> UpdateNoticeAsync(UpdateEmergencyNoticeRequest request);
        Task<EmergencyNoticeResponse> ToggleNoticeAsync(ToggleEmergencyNoticeRequest request);
        Task<EmergencyNoticeResponse> DeleteNoticeAsync(int id);
        Task<GetActiveNoticesForDepartmentResponse> GetActiveNoticesForDepartmentAsync(int departmentId);
    }
}
