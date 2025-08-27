// ===============================
// MessageManager/Services/IEmergencyNoticeApiService.cs - クライアント側
// ===============================
using MessageManager.Models;
using System.Threading.Tasks;

namespace MessageManager.Services
{
    public interface IEmergencyNoticeApiService
    {
        Task<GetEmergencyNoticesResponse> GetAllNoticesAsync();
        Task<EmergencyNoticeResponse> CreateNoticeAsync(CreateEmergencyNoticeRequest request);
        Task<EmergencyNoticeResponse> UpdateNoticeAsync(UpdateEmergencyNoticeRequest request);
        Task<EmergencyNoticeResponse> ToggleNoticeAsync(ToggleEmergencyNoticeRequest request);
        Task<EmergencyNoticeResponse> DeleteNoticeAsync(int id);
        Task<GetActiveNoticesForDepartmentResponse> GetActiveNoticesForDepartmentAsync(int departmentId);
    }
}
