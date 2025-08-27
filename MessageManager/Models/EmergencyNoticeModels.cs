// ===============================
// Models/EmergencyNoticeModels.cs
// ===============================
using MessageManager.Services;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MessageManager.Models
{
    /// <summary>
    /// 緊急連絡事項
    /// </summary>
    public class EmergencyNotice
    {
        public int Id { get; set; }
        public Priority Priority { get; set; }
        public string NoticeType { get; set; } = "";
        public string NoticeContent { get; set; } = "";
        public List<int>? TargetDepartments { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreatedByDepartmentId { get; set; }

        // 表示用プロパティ
        public string PriorityDisplay => GetPriorityDisplay(Priority);
        public string PriorityIcon => GetPriorityIcon(Priority);
        public string TargetDepartmentNames { get; set; } = "";
        public string CreatedByDepartmentName { get; set; } = "";
        public string StatusDisplay => IsActive ? "有効" : "終了";
        public string StatusColor => IsActive ? "#28a745" : "#6c757d";

        private static string GetPriorityDisplay(Priority priority) => priority switch
        {
            Priority.Urgent => "緊急",
            Priority.High => "重要",
            Priority.Normal => "通常",
            Priority.Low => "参考",
            _ => "通常"
        };

        private static string GetPriorityIcon(Priority priority) => priority switch
        {
            Priority.Urgent => "🔴",
            Priority.High => "🟡",
            Priority.Normal => "🟢",
            Priority.Low => "🔵",
            _ => "🟢"
        };
    }

    /// <summary>
    /// 緊急連絡事項作成リクエスト
    /// </summary>
    public class CreateEmergencyNoticeRequest
    {
        public Priority Priority { get; set; }
        public string NoticeType { get; set; } = "";
        public string NoticeContent { get; set; } = "";
        public List<int>? TargetDepartments { get; set; }
        public int CreatedByDepartmentId { get; set; }
    }

    /// <summary>
    /// 緊急連絡事項更新リクエスト
    /// </summary>
    public class UpdateEmergencyNoticeRequest
    {
        public int Id { get; set; }
        public Priority Priority { get; set; }
        public string NoticeType { get; set; } = "";
        public string NoticeContent { get; set; } = "";
        public List<int>? TargetDepartments { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 緊急連絡事項の有効/無効切り替えリクエスト
    /// </summary>
    public class ToggleEmergencyNoticeRequest
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 緊急連絡事項一覧取得レスポンス
    /// </summary>
    public class GetEmergencyNoticesResponse : IApiResponse
    {
        public List<EmergencyNotice> Notices { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// 緊急連絡事項作成/更新レスポンス
    /// </summary>
    public class EmergencyNoticeResponse : IApiResponse
    {
        public EmergencyNotice? Notice { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// 部署別有効緊急連絡事項取得レスポンス
    /// </summary>
    public class GetActiveNoticesForDepartmentResponse : IApiResponse
    {
        public List<EmergencyNotice> Notices { get; set; } = new();
        public string CombinedContent { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}

