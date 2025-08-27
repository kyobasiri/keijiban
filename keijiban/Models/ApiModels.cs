using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace keijiban.Models
{
    // C# 9.0以降で利用可能な`init`セッターを使用しています。
    // これにより、オブジェクト作成時にプロパティを初期化でき、その後は変更不可（イミュータブル）になります。
    // データの不整合を防ぎ、安全なコードになります。

    #region General API Response

    /// <summary>
    /// APIレスポンスの共通ベースクラス。
    /// </summary>
    public record ApiResponseBase
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    #endregion

    #region Department and Lists

    /// <summary>
    /// 部署やグループを表す基本的なデータモデル。
    /// </summary>
    public record Department
    {
        public int? Id { get; init; }
        public string? Name { get; init; } = string.Empty;
    }

    /// <summary>
    /// 部署リスト取得API (/schedule/lists) のレスポンス。
    /// </summary>
    public record DepartmentListsApiResponse : ApiResponseBase
    {
        public List<Department> SchedulegroupViewDepartments { get; init; } = new();
        public List<Department> ViewDepartments { get; init; } = new();
    }

    #endregion

    #region Schedule

    /// <summary>
    /// スケジュール情報の単一項目。
    /// </summary>
    public record ScheduleApiItem
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string? StartTime { get; init; }
        public string? EndTime { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Department { get; init; }
    }

    /// <summary>
    /// スケジュール取得API (/schedule/group, /schedule/department) のレスポンス。
    /// </summary>
    public record ScheduleApiResponse : ApiResponseBase
    {
        public List<ScheduleApiItem> Schedules { get; init; } = new();
    }

    /// <summary>
    /// その他スケジュール情報の単一項目。
    /// </summary>
    public record ExtraScheduleDataApiItem
    {
        public DateTime ScheduleDate { get; init; }
        public string Category { get; init; } = string.Empty;
        public string? StartTime { get; init; }
        public string? EndTime { get; init; }
        public string Title { get; init; } = string.Empty;
    }

    /// <summary>
    /// その他スケジュールデータ取得API (/schedule/extra-data) のレスポンス。
    /// </summary>
    public record ExtraScheduleDataApiResponse : ApiResponseBase
    {
        public List<ExtraScheduleDataApiItem> ExtraSchedules { get; init; } = new();
    }

    #endregion

    #region Doctor Absence

    /// <summary>
    /// 医師の不在情報。
    /// </summary>
    public record DoctorAbsenceApiItem
    {
        public string? StartTime { get; init; }
        public string? EndTime { get; init; }
        public string Detail { get; init; } = string.Empty;
        public string? MiniDetail { get; init; }
        public string? Reason { get; init; }
    }

    /// <summary>
    /// 医師不在情報取得API (/schedule/doctor-absences) のレスポンス。
    /// </summary>
    public record DoctorAbsenceApiResponse : ApiResponseBase
    {
        public Dictionary<string, List<DoctorAbsenceApiItem>> DoctorSchedules { get; init; } = new();
    }

    #endregion

    #region Information

    /// <summary>
    /// インフォメーション情報の単一項目。
    /// </summary>
    public record InformationApiItem
    {
        public DateTime PostDate { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }

    /// <summary>
    /// インフォメーション取得API (/schedule/information) のレスポンス。
    /// </summary>
    public record InformationApiResponse : ApiResponseBase
    {
        public List<InformationApiItem> Information { get; init; } = new();
    }

    #endregion

    #region Emergency Notice

    /// <summary>
    /// 緊急情報の重要度。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmergencyPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    /// <summary>
    /// 緊急情報の単一項目。
    /// </summary>
    public record EmergencyNoticeApiItem
    {
        public int Id { get; init; }
        public string NoticeType { get; init; } = string.Empty;
        public string NoticeContent { get; init; } = string.Empty;
        public EmergencyPriority Priority { get; init; }
        public bool IsActive { get; init; }
        public bool IsAllDepartments { get; init; }
        public List<int> TargetDepartmentIds { get; init; } = new();
    }

    /// <summary>
    /// 緊急情報取得API (/emergencynotice/...) のレスポンス。
    /// </summary>
    public record GetActiveNoticesForDepartmentResponse : ApiResponseBase
    {
        public List<EmergencyNoticeApiItem> Notices { get; init; } = new();
    }

    #endregion

    #region Message

    /// <summary>
    /// 部署間連絡メッセージの重要度。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessagePriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    /// <summary>
    /// 部署間連絡メッセージのリスト項目。
    /// </summary>
    public record MessageListApiItem
    {
        public int MessageId { get; init; }
        public string Subject { get; init; } = string.Empty;
        public string FromDeptName { get; init; } = string.Empty;
        public string ToDeptNames { get; init; } = string.Empty;
        public MessagePriority Priority { get; init; }
        public string PriorityIcon { get; init; } = string.Empty;
        public string DateDisplay { get; init; } = string.Empty;
        public string DueDateDisplay { get; init; } = string.Empty;
        public string StatusDisplay { get; init; } = string.Empty;
        public string StatusColor { get; init; } = string.Empty;
        public bool IsRead { get; init; }
    }

    /// <summary>
    /// 部署間連絡メッセージリスト取得API (/message/...) のレスポンス。
    /// </summary>
    public record MessageListApiResponse : ApiResponseBase
    {
        public List<MessageListApiItem> Messages { get; init; } = new();
    }

    #endregion
}
