// =============================================
// Repositories/IEmergencyNoticeRepository.cs
// =============================================
using keijibanapi.Models;

namespace keijibanapi.Repositories
{
    /// <summary>
    /// 緊急連絡事項データへのアクセスを定義します。
    /// </summary>
    public interface IEmergencyNoticeRepository
    {
        Task<IEnumerable<EmergencyNotice>> GetAllWithDepartmentsAsync();
        Task<IEnumerable<EmergencyNotice>> GetActiveNoticesForDepartmentAsync(int departmentId);
        Task<EmergencyNotice?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateEmergencyNoticeRequest request);
        Task<bool> UpdateAsync(UpdateEmergencyNoticeRequest request);
        Task<bool> ToggleAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}
