// =======================================
// Repositories/IDepartmentRepository.cs
// =======================================
using keijibanapi.Models;

namespace keijibanapi.Repositories
{
    /// <summary>
    /// 部署データへのアクセスを定義します。
    /// </summary>
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetActiveDepartmentsAsync(int? displaycase = null);
        Task<Dictionary<int, string>> GetDepartmentNamesDictionaryAsync();
        Task<IEnumerable<DepartmentMaster>> GetAllDepartmentMastersAsync();
        Task<bool> UpdateDepartmentMasterAsync(UpdateDepartmentMasterRequest request);
        Task<bool> IsDepartmentIdDuplicateAsync(int id, int? departmentId);
    }
}
