// =======================================
// Repositories/IScheduleRepository.cs
// =======================================
using keijibanapi.Models;

namespace keijibanapi.Repositories
{
    /// <summary>
    /// スケジュールおよびインフォメーションのキャッシュデータへのアクセスを定義します。
    /// </summary>
    public interface IScheduleRepository
    {
        Task<IEnumerable<ScheduleItem>> GetScheduleGroupDataAsync(DateTime startDate, DateTime endDate, int? departmentId = null);
        Task<IEnumerable<ScheduleItem>> GetDepartmentScheduleDataAsync(DateTime startDate, DateTime endDate, int? departmentId = null);
        Task<IEnumerable<InformationApiItem>> GetInformationAsync(int? departmentId = null);
    }
}
