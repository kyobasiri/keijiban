// ==========================================
// Repositories/IExtraScheduleRepository.cs
// ==========================================
using keijibanapi.Models;

namespace keijibanapi.Repositories
{
    /// <summary>
    /// その他スケジュールデータへのアクセスを定義します。
    /// </summary>
    public interface IExtraScheduleRepository
    {
        Task<IEnumerable<ExtraScheduleData>> GetExtraScheduleDataAsync(DateTime startDate, DateTime endDate, int? departmentId = null);
    }
}
