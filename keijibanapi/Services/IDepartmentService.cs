// =======================================
// Services/IDepartmentService.cs - 最終形
// =======================================
using keijibanapi.Models;

namespace keijibanapi.Services
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetActiveDepartmentsAsync(int? displaycase = null);
        Task<DepartmentListsResponse> GetDepartmentListsAsync();
        Task<Dictionary<int, string>> GetDepartmentNamesDictionaryAsync();

        // ★ ScheduleServiceから移管
        Task<DepartmentMasterListResponse> GetAllDepartmentMastersAsync();
        Task<UpdateDepartmentMasterResponse> UpdateDepartmentMasterAsync(UpdateDepartmentMasterRequest request);
    }
}
