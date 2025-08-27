using keijiban.Models;
using System;
using System.Threading.Tasks;

namespace keijiban.Services
{
    /// <summary>
    /// バックエンドAPIとの通信を担うサービスのインターフェース。
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// 指定されたスケジュールグループのスケジュールデータを取得します。
        /// </summary>
        /// <param name="startDate">取得を開始する日付。</param>
        /// <param name="departmentId">対象の部署ID。nullの場合は全グループを対象とします。</param>
        /// <returns>スケジュールのAPIレスポンス。</returns>
        Task<ScheduleApiResponse> GetScheduleGroupDataAsync(DateTime startDate, int? departmentId = null);

        /// <summary>
        /// 指定された部署のスケジュールデータを取得します。
        /// </summary>
        /// <param name="startDate">取得を開始する日付。</param>
        /// <param name="departmentId">対象の部署ID。</param>
        /// <returns>スケジュールのAPIレスポンス。</returns>
        Task<ScheduleApiResponse> GetDepartmentScheduleDataAsync(DateTime startDate, int? departmentId = null);

        /// <summary>
        /// 指定された部署のその他スケジュールデータ（情報タブ用）を取得します。
        /// </summary>
        /// <param name="departmentId">対象の部署ID。</param>
        /// <param name="startDate">取得を開始する日付。</param>
        /// <returns>その他スケジュールのAPIレスポンス。</returns>
        Task<ExtraScheduleDataApiResponse> GetExtraScheduleDataAsync(int? departmentId = null, DateTime? startDate = null);

        /// <summary>
        /// 医師の不在予定を取得します。
        /// </summary>
        /// <param name="startDate">取得を開始する日付。</param>
        /// <returns>医師不在予定のAPIレスポンス。</returns>
        Task<DoctorAbsenceApiResponse> GetDoctorAbsencesAsync(DateTime startDate);

        /// <summary>
        /// スケジュール表示に必要な部署とグループの一覧を取得します。
        /// </summary>
        /// <returns>部署リストのAPIレスポンス。</returns>
        Task<DepartmentListsApiResponse> GetDepartmentListsAsync();

        /// <summary>
        /// インフォメーション（お知らせ）を取得します。
        /// </summary>
        /// <param name="departmentId">対象の部署ID。nullの場合は全部署向けを対象とします。</param>
        /// <returns>インフォメーションのAPIレスポンス。</returns>
        Task<InformationApiResponse> GetInformationAsync(int? departmentId = null);

        /// <summary>
        /// 指定された部署とスケジュールグループに関連する、現在有効な緊急情報をすべて取得します。
        /// </summary>
        /// <param name="scheduleGroupDepartmentId">スケジュールグループとして選択されている部署のID。</param>
        /// <param name="displayDepartmentId">表示部署として選択されている部署のID。</param>
        /// <returns>有効な緊急情報リストを含むAPIレスポンス。</returns>
        Task<GetActiveNoticesForDepartmentResponse> GetCombinedActiveNoticesForDepartmentsAsync(int? scheduleGroupDepartmentId, int? displayDepartmentId);

        /// <summary>
        /// 指定された部署が送信したメッセージの一覧を取得します。
        /// </summary>
        /// <param name="fromDeptId">送信元の部署ID。</param>
        /// <param name="limit">取得する最大件数。</param>
        /// <returns>送信メッセージリストのAPIレスポンス。</returns>
        Task<MessageListApiResponse> GetSentMessagesAsync(int? fromDeptId, int limit = 10);

        /// <summary>
        /// 指定された部署が受信したメッセージの一覧を取得します。
        /// </summary>
        /// <param name="toDeptId">受信先の部署ID。</param>
        /// <param name="limit">取得する最大件数。</param>
        /// <returns>受信メッセージリストのAPIレスポンス。</returns>
        Task<MessageListApiResponse> GetReceivedMessagesAsync(int? toDeptId, int limit = 10);
    }
}
