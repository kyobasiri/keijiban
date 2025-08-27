
// ===============================
// Models/ScheduleModels.cs
// ===============================
namespace keijibanapi.Models
{
    public class ScheduleItem
    {
        public string Title { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Department { get; set; } = "";
        public int? DepartmentId { get; set; }
    }

    public class DoctorAbsence
    {
        public string DoctorName { get; set; } = "";
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Reason { get; set; } = "";
        public string Detail { get; set; } = "";
        public string MiniDetail { get; set; } = "";
    }

    public class DoctorAbsenceResponse
    {
        public Dictionary<string, List<DoctorAbsence>> DoctorSchedules { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class ScheduleResponse
    {
        public List<ScheduleItem> Schedules { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class Department
    {
        public int? Id { get; set; }
        public string? Name { get; set; } = "";
    }

    public class DepartmentApiResponse
    {
        public List<Department> Departments { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class InformationApiItem
    {
        public string Title { get; set; } = "";
        public DateTime PostDate { get; set; }
        public string Content { get; set; } = "";
        public string Author { get; set; } = "";
    }

    public class InformationApiResponse
    {
        public List<InformationApiItem> Information { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class ScheduleRequest
    {
        public DateTime StartDate { get; set; }
        public string? ScheduleGroup { get; set; }
        public int? DepartmentId { get; set; }
    }

    // ★新規追加: 2つの部署リストを持つレスポンス用のモデル
    public class DepartmentListsResponse
    {
        // プロパティ名はクライアントの用途に合わせると分かりやすい
        // 例: スケジュール表示用、メッセージ送信用など
        public List<Department> SchedulegroupViewDepartments { get; set; } = new();
        public List<Department> ViewDepartments { get; set; } = new();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
