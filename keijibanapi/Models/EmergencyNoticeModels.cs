// ===============================
// keijibanapi/Models/EmergencyNoticeModels.cs - API側専用
// ===============================
using System;
using System.Collections.Generic;

namespace keijibanapi.Models
{
    /// <summary>
    /// 緊急連絡事項専用の重要度列挙型
    /// </summary>
    public enum EmergencyPriority
    {
        Low,
        Normal,
        High,
        Urgent
        // 将来的に Critical や Immediate などを追加可能
    }

    /// <summary>
    /// API側緊急連絡事項
    /// </summary>
    public class EmergencyNotice
    {
        public int Id { get; set; }
        public EmergencyPriority Priority { get; set; } // 専用のPriorityを使用
        public string NoticeType { get; set; } = "";
        public string NoticeContent { get; set; } = "";
        public List<int>? TargetDepartments { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreatedByDepartmentId { get; set; }
        public string TargetDepartmentNames { get; set; } = "";
        public string CreatedByDepartmentName { get; set; } = "";
    }

    public class CreateEmergencyNoticeRequest
    {
        public EmergencyPriority Priority { get; set; }
        public string NoticeType { get; set; } = "";
        public string NoticeContent { get; set; } = "";
        public List<int>? TargetDepartments { get; set; }
        public int CreatedByDepartmentId { get; set; }
    }

    public class UpdateEmergencyNoticeRequest
    {
        public int Id { get; set; }
        public EmergencyPriority Priority { get; set; }
        public string NoticeType { get; set; } = "";
        public string NoticeContent { get; set; } = "";
        public List<int>? TargetDepartments { get; set; }
        public bool IsActive { get; set; }
    }

    public class ToggleEmergencyNoticeRequest
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetEmergencyNoticesResponse
    {
        public List<EmergencyNotice> Notices { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class EmergencyNoticeResponse
    {
        public EmergencyNotice? Notice { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class GetActiveNoticesForDepartmentResponse
    {
        public List<EmergencyNotice> Notices { get; set; } = new();
        public string CombinedContent { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
