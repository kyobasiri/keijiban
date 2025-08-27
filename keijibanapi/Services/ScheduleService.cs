// =======================================
// Services/ScheduleService.cs - リファクタリング後
// =======================================
using keijibanapi.Models;
using keijibanapi.Repositories;

namespace keijibanapi.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(IScheduleRepository scheduleRepository, ILogger<ScheduleService> logger)
        {
            _scheduleRepository = scheduleRepository;
            _logger = logger;
        }

        public async Task<ScheduleResponse> GetScheduleGroupDataAsync(DateTime startDate, int? departmentId = null)
        {
            try
            {
                var targetEndDate = startDate.AddDays(7);
                var schedules = await _scheduleRepository.GetScheduleGroupDataAsync(startDate, targetEndDate, departmentId);
                return new ScheduleResponse
                {
                    Schedules = schedules.ToList(),
                    Success = true,
                    Message = "スケジュールグループデータを正常に取得しました"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule group data for department {DepartmentId}", departmentId);
                return new ScheduleResponse { Success = false, Message = $"スケジュールグループデータの取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<ScheduleResponse> GetDepartmentScheduleDataAsync(DateTime startDate, int? departmentId = null)
        {
            try
            {
                var targetEndDate = startDate.AddDays(7);
                var schedules = await _scheduleRepository.GetDepartmentScheduleDataAsync(startDate, targetEndDate, departmentId);
                return new ScheduleResponse
                {
                    Schedules = schedules.ToList(),
                    Success = true,
                    Message = "部署スケジュールデータを正常に取得しました"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department schedule data for department {DepartmentId}", departmentId);
                return new ScheduleResponse { Success = false, Message = $"部署スケジュールデータの取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<InformationApiResponse> GetInformationAsync(int? departmentId = null)
        {
            try
            {
                var information = await _scheduleRepository.GetInformationAsync(departmentId);
                return new InformationApiResponse
                {
                    Information = information.ToList(),
                    Success = true,
                    Message = "インフォメーションデータを正常に取得しました"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving information data");
                return new InformationApiResponse { Success = false, Message = $"インフォメーションデータの取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<ScheduleResponse> GetCombinedScheduleGroupDataAsync(DateTime startDate, int? departmentId = null)
        {
            try
            {
                var tasks = new List<Task<IEnumerable<ScheduleItem>>>();
                var targetEndDate = startDate.AddDays(7);

                // 1. 選択されたグループのデータを取得するタスクを追加
                if (departmentId.HasValue && departmentId.Value > 0)
                {
                    tasks.Add(_scheduleRepository.GetScheduleGroupDataAsync(startDate, targetEndDate, departmentId.Value));
                }

                // 2. 清和会(ID:100)のデータを取得するタスクを追加 (重複しない場合)
                if (!departmentId.HasValue || departmentId.Value != 100)
                {
                    tasks.Add(_scheduleRepository.GetScheduleGroupDataAsync(startDate, targetEndDate, 100));
                }

                if (!tasks.Any())
                {
                    return new ScheduleResponse { Schedules = new List<ScheduleItem>(), Success = true, Message = "対象データがありません" };
                }

                // 複数のDBアクセスを並列で実行
                var results = await Task.WhenAll(tasks);

                // 結果をマージし、ソートする
                var combinedSchedules = results
                    .SelectMany(schedules => schedules) // 結果のリストを平坦化
                    .OrderBy(s => s.StartDate)
                    .ThenBy(s => s.StartTime)
                    .ThenBy(s => s.Title)
                    .ToList();

                return new ScheduleResponse
                {
                    Schedules = combinedSchedules,
                    Success = true,
                    Message = "スケジュールグループデータを正常に取得しました"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving combined schedule group data for department {DepartmentId}", departmentId);
                return new ScheduleResponse { Success = false, Message = $"統合スケジュールグループデータの取得に失敗しました: {ex.Message}" };
            }
        }
        // 部署マスタ関連のメソッドは DepartmentService に移管したため、ここからは削除します。
        // GetExternalDepartmentsAsync も Obsolete のため削除します。
    }
}
