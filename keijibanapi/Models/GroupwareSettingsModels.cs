// ===============================
// Models/GroupwareSettingsModels.cs - 簡素化版（完全版）
// ===============================
using System.Collections.ObjectModel;

namespace keijibanapi.Models
{
    /// <summary>
    /// 部署マスタ（MariaDB）
    /// </summary>
    public class DepartmentMaster
    {
        public int Id { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = "";
        public string ExternalSystemName { get; set; } = "";
        public string? ExternalDepartmentId { get; set; } // varchar(50)に変更
        public string ExternalDepartmentName { get; set; } = "";
        public byte DisplayCase { get; set; } = 3; // 新規追加：1=スケジュールグループ専用, 2=部署スケジュール専用, 3=両方
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 外部システム部署情報（削除予定 - シェルに移管）
    /// </summary>
    [Obsolete("External department sync should be handled by shell scripts")]
    public class ExternalDepartment
    {
        public string ExternalDepartmentId { get; set; } = ""; // varchar(50)に変更
        public string ExternalDepartmentName { get; set; } = "";
        public string SystemName { get; set; } = "";
    }

    /// <summary>
    /// 部署同期レスポンス（削除予定 - シェルに移管）
    /// </summary>
    [Obsolete("External department sync should be handled by shell scripts")]
    public class DepartmentSyncResponse
    {
        public List<ExternalDepartment> ExternalDepartments { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// 部署マスタ一覧レスポンス（マスタメンテナンス用）
    /// </summary>
    public class DepartmentMasterListResponse
    {
        public List<DepartmentMaster> DepartmentMasters { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }



    /// <summary>
    /// 部署マスタ更新リクエスト
    /// </summary>
    public class UpdateDepartmentMasterRequest
    {
        public int Id { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = "";
        public byte DisplayCase { get; set; } = 3; // 新規追加：1=スケジュールグループ専用, 2=部署スケジュール専用, 3=両方
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 部署マスタ更新レスポンス
    /// </summary>
    public class UpdateDepartmentMasterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
