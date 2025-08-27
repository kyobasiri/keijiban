// =======================================
// Services/DepartmentService.cs - リファクタリング後
// =======================================
using keijibanapi.Models;
using keijibanapi.Repositories;

namespace keijibanapi.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IDepartmentRepository departmentRepository, ILogger<DepartmentService> logger)
        {
            _departmentRepository = departmentRepository;
            _logger = logger;
        }

        public async Task<List<Department>> GetActiveDepartmentsAsync(int? displaycase = null)
        {
            try
            {
                var departments = await _departmentRepository.GetActiveDepartmentsAsync(displaycase);
                return departments.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active departments.");
                return new List<Department>();
            }
        }

        public async Task<DepartmentListsResponse> GetDepartmentListsAsync()
        {
            var response = new DepartmentListsResponse();
            try
            {
                var scheduleGroupDepts = await _departmentRepository.GetActiveDepartmentsAsync(1);
                var viewDepts = await _departmentRepository.GetActiveDepartmentsAsync(2);

                response.SchedulegroupViewDepartments = scheduleGroupDepts.ToList();
                response.ViewDepartments = viewDepts.ToList();
                response.Success = true;
                _logger.LogInformation("Successfully retrieved department lists for both views.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department lists.");
                response.Success = false;
                response.Message = "部署リストの取得中にエラーが発生しました。";
            }
            return response;
        }

        public async Task<Dictionary<int, string>> GetDepartmentNamesDictionaryAsync()
        {
            try
            {
                return await _departmentRepository.GetDepartmentNamesDictionaryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department names dictionary.");
                return new Dictionary<int, string>();
            }
        }

        // ★ ScheduleServiceから移管されたメソッド (ここから)
        public async Task<DepartmentMasterListResponse> GetAllDepartmentMastersAsync()
        {
            try
            {
                var masters = await _departmentRepository.GetAllDepartmentMastersAsync();
                return new DepartmentMasterListResponse
                {
                    DepartmentMasters = masters.ToList(),
                    Success = true,
                    Message = "部署マスタデータを正常に取得しました"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all department masters.");
                return new DepartmentMasterListResponse { Success = false, Message = $"部署マスタデータの取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<UpdateDepartmentMasterResponse> UpdateDepartmentMasterAsync(UpdateDepartmentMasterRequest request)
        {
            try
            {
                // 重複チェック
                if (await _departmentRepository.IsDepartmentIdDuplicateAsync(request.Id, request.DepartmentId))
                {
                    return new UpdateDepartmentMasterResponse { Success = false, Message = "同じ外部システムで指定された部署IDは既に存在します" };
                }

                var success = await _departmentRepository.UpdateDepartmentMasterAsync(request);
                if (success)
                {
                    return new UpdateDepartmentMasterResponse { Success = true, Message = "部署マスタの更新が完了しました" };
                }
                else
                {
                    return new UpdateDepartmentMasterResponse { Success = false, Message = "指定された部署マスタが見つかりません" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department master ID: {Id}", request.Id);
                return new UpdateDepartmentMasterResponse { Success = false, Message = $"部署マスタの更新に失敗しました: {ex.Message}" };
            }
        }
        // ★ ScheduleServiceから移管されたメソッド (ここまで)
    }
}
