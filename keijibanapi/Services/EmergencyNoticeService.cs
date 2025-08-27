// =============================================
// Services/EmergencyNoticeService.cs - リファクタリング後
// =============================================
using keijibanapi.Models;
using keijibanapi.Repositories;

namespace keijibanapi.Services
{
    public class EmergencyNoticeService : IEmergencyNoticeService
    {
        private readonly IEmergencyNoticeRepository _emergencyNoticeRepository;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<EmergencyNoticeService> _logger;

        public EmergencyNoticeService(IEmergencyNoticeRepository emergencyNoticeRepository, IDepartmentService departmentService, ILogger<EmergencyNoticeService> logger)
        {
            _emergencyNoticeRepository = emergencyNoticeRepository;
            _departmentService = departmentService;
            _logger = logger;
        }

        public async Task<GetEmergencyNoticesResponse> GetAllNoticesAsync()
        {
            try
            {
                // リポジトリがN+1問題を解決済みなので、取得するだけで良い
                var notices = await _emergencyNoticeRepository.GetAllWithDepartmentsAsync();
                var departmentNames = await _departmentService.GetDepartmentNamesDictionaryAsync();

                foreach (var notice in notices)
                {
                    if (notice.CreatedByDepartmentId.HasValue)
                    {
                        notice.CreatedByDepartmentName = departmentNames.GetValueOrDefault(notice.CreatedByDepartmentId.Value, "");
                    }
                }

                return new GetEmergencyNoticesResponse { Notices = notices.ToList(), Success = true, Message = "緊急連絡事項を正常に取得しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency notices");
                return new GetEmergencyNoticesResponse { Success = false, Message = $"緊急連絡事項の取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<GetActiveNoticesForDepartmentResponse> GetActiveNoticesForDepartmentAsync(int departmentId)
        {
            try
            {
                var notices = (await _emergencyNoticeRepository.GetActiveNoticesForDepartmentAsync(departmentId)).ToList();
                var departmentNames = await _departmentService.GetDepartmentNamesDictionaryAsync();
                foreach (var notice in notices)
                {
                    if (notice.CreatedByDepartmentId.HasValue)
                        notice.CreatedByDepartmentName = departmentNames.GetValueOrDefault(notice.CreatedByDepartmentId.Value, "");
                }

                var combinedContent = string.Join(" | ", notices.Select(n => $"【{n.NoticeType}】{n.NoticeContent}"));

                return new GetActiveNoticesForDepartmentResponse { Notices = notices, CombinedContent = combinedContent, Success = true, Message = "部署別有効緊急連絡事項を正常に取得しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active emergency notices for department {DepartmentId}", departmentId);
                return new GetActiveNoticesForDepartmentResponse { Success = false, Message = $"部署別緊急連絡事項の取得に失敗しました: {ex.Message}" };
            }
        }

        public async Task<EmergencyNoticeResponse> CreateNoticeAsync(CreateEmergencyNoticeRequest request)
        {
            try
            {
                var newId = await _emergencyNoticeRepository.CreateAsync(request);
                var createdNotice = await _emergencyNoticeRepository.GetByIdAsync(newId);

                // 作成者部署名を手動で設定
                if (createdNotice != null && createdNotice.CreatedByDepartmentId.HasValue)
                {
                    var deptNames = await _departmentService.GetDepartmentNamesDictionaryAsync();
                    createdNotice.CreatedByDepartmentName = deptNames.GetValueOrDefault(createdNotice.CreatedByDepartmentId.Value, "");
                }

                return new EmergencyNoticeResponse { Notice = createdNotice, Success = true, Message = "緊急連絡事項を作成しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency notice");
                return new EmergencyNoticeResponse { Success = false, Message = $"緊急連絡事項の作成に失敗しました: {ex.Message}" };
            }
        }

        public async Task<EmergencyNoticeResponse> UpdateNoticeAsync(UpdateEmergencyNoticeRequest request)
        {
            try
            {
                var success = await _emergencyNoticeRepository.UpdateAsync(request);
                if (!success) return new EmergencyNoticeResponse { Success = false, Message = "指定されたIDの緊急連絡事項が見つかりません" };

                var updatedNotice = await _emergencyNoticeRepository.GetByIdAsync(request.Id);
                if (updatedNotice != null && updatedNotice.CreatedByDepartmentId.HasValue)
                {
                    var deptNames = await _departmentService.GetDepartmentNamesDictionaryAsync();
                    updatedNotice.CreatedByDepartmentName = deptNames.GetValueOrDefault(updatedNotice.CreatedByDepartmentId.Value, "");
                }

                return new EmergencyNoticeResponse { Notice = updatedNotice, Success = true, Message = "緊急連絡事項を更新しました" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emergency notice");
                return new EmergencyNoticeResponse { Success = false, Message = $"緊急連絡事項の更新に失敗しました: {ex.Message}" };
            }
        }

        public async Task<EmergencyNoticeResponse> ToggleNoticeAsync(ToggleEmergencyNoticeRequest request)
        {
            try
            {
                var success = await _emergencyNoticeRepository.ToggleAsync(request.Id, request.IsActive);
                return new EmergencyNoticeResponse
                {
                    Success = success,
                    Message = success ? $"緊急連絡事項を{(request.IsActive ? "有効" : "無効")}にしました" : "指定されたIDの緊急連絡事項が見つかりません"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling emergency notice");
                return new EmergencyNoticeResponse { Success = false, Message = $"緊急連絡事項の状態変更に失敗しました: {ex.Message}" };
            }
        }

        public async Task<EmergencyNoticeResponse> DeleteNoticeAsync(int id)
        {
            try
            {
                var success = await _emergencyNoticeRepository.DeleteAsync(id);
                return new EmergencyNoticeResponse
                {
                    Success = success,
                    Message = success ? "緊急連絡事項を削除しました" : "指定されたIDの緊急連絡事項が見つかりません"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting emergency notice");
                return new EmergencyNoticeResponse { Success = false, Message = $"緊急連絡事項の削除に失敗しました: {ex.Message}" };
            }
        }
    }
}
