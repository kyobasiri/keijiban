// ===============================
// MessageManager/Models/DepartmentMaintenanceModels.cs - DisplayCase対応版
// ===============================
using MessageManager.Services;
using System;
using System.Collections.Generic;

namespace MessageManager.Models
{
    /// <summary>
    /// 部署マスタ更新用リクエスト
    /// </summary>
    public class UpdateDepartmentMasterRequest
    {
        public int Id { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = "";
        public byte DisplayCase { get; set; } = 3; // ★ 新規追加
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 部署マスタ更新用レスポンス
    /// </summary>
    public class UpdateDepartmentMasterResponse : IApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }



    /// <summary>
    /// 部署マスタ一覧取得用レスポンス（APIのDepartmentMasterListResponseに対応）
    /// </summary>
    public class GetAllDepartmentMastersResponse : IApiResponse
    {
        public List<DepartmentMaster> DepartmentMasters { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// API側のDepartmentMasterListResponseのエイリアス
    /// </summary>
    public class DepartmentMasterListResponse : GetAllDepartmentMastersResponse
    {
    }

    /// <summary>
    /// 部署マスタのデータを表すモデル
    /// </summary>
    public class DepartmentMaster
    {
        public int Id { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = "";
        public string ExternalSystemName { get; set; } = "";
        public string? ExternalDepartmentId { get; set; } // ★ string?に変更（APIに合わせる）
        public string ExternalDepartmentName { get; set; } = "";
        public byte DisplayCase { get; set; } = 3; // ★ 新規追加：1=1行目専用, 2=2行目専用, 3=両方
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
