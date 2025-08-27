// ===============================
// Services/IExtraScheduleService.cs - その他スケジュールデータサービスインターフェース
// ===============================
using keijibanapi.Models;

namespace keijibanapi.Services
{
    /// <summary>
    /// その他スケジュールデータサービスのインターフェース
    /// </summary>
    public interface IExtraScheduleService
    {
        /// <summary>
        /// その他スケジュールデータを取得します
        /// </summary>
        /// <param name="departmentId">部署ID（指定しない場合は全データ）</param>
        /// <param name="startDate">開始日（指定しない場合は当日から7日間）</param>
        /// <returns>その他スケジュールデータのリスト</returns>
        Task<ExtraScheduleDataResponse> GetExtraScheduleDataAsync(int? departmentId = null, DateTime? startDate = null);
    }
}
