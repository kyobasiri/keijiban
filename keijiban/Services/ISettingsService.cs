using System.Threading.Tasks;

namespace keijiban.Services
{
    /// <summary>
    /// ユーザーの選択設定を永続化および取得するためのサービスのインターフェース。
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 保存されている選択グループIDを非同期に取得します。
        /// </summary>
        /// <returns>保存されているグループID。存在しない場合はデフォルト値。</returns>
        Task<int> GetSelectedGroupIdAsync();

        /// <summary>
        /// 保存されている選択部署IDを非同期に取得します。
        /// </summary>
        /// <returns>保存されている部署ID。存在しない場合はデフォルト値。</returns>
        Task<int> GetSelectedDepartmentIdAsync();

        /// <summary>
        /// 保存されている選択情報種別IDを非同期に取得します。
        /// </summary>
        /// <returns>保存されている情報種別ID。存在しない場合はデフォルト値。</returns>
        Task<int> GetSelectedInfoTypeIdAsync();

        /// <summary>
        /// 選択されたグループIDを非同期に保存します。
        /// </summary>
        /// <param name="groupId">保存するグループID。</param>
        Task SaveSelectedGroupIdAsync(int? groupId);

        /// <summary>
        /// 選択された部署IDを非同期に保存します。
        /// </summary>
        /// <param name="departmentId">保存する部署ID。</param>
        Task SaveSelectedDepartmentIdAsync(int? departmentId);

        /// <summary>
        /// 選択された情報種別IDを非同期に保存します。
        /// </summary>
        /// <param name="infoTypeId">保存する情報種別ID。</param>
        Task SaveSelectedInfoTypeIdAsync(int infoTypeId);
    }
}
