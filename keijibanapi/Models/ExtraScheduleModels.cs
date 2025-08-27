// ===============================
// Models/ExtraScheduleModels.cs - その他スケジュールデータ用モデル
// ===============================
namespace keijibanapi.Models
{
    /// <summary>
    /// その他スケジュールデータ
    /// </summary>
    public class ExtraScheduleData
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// その他スケジュールデータ取得レスポンス
    /// </summary>
    public class ExtraScheduleDataResponse
    {
        public List<ExtraScheduleData> ExtraSchedules { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// キャッシュ用スケジュールグループデータ
    /// </summary>
    public class ScheduleGroupCacheData
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public int DepartmentId { get; set; }
        public string ExternalDepartmentId { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// キャッシュ用部署スケジュールデータ
    /// </summary>
    public class DepartmentScheduleCacheData
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public int DepartmentId { get; set; }
        public string ExternalDepartmentId { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// キャッシュ用インフォメーションデータ
    /// </summary>
    public class InformationCacheData
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime PostDate { get; set; }
        public string Content { get; set; } = "";
        public string Author { get; set; } = "";
        public int? DepartmentId { get; set; } // NULLの場合は全部署用
        public string? ExternalDepartmentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
