// ===============================
// MessageManager/ViewModels/EmergencyNoticeViewModel.cs - Window参照修正版
// ===============================
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using MessageManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace MessageManager.ViewModels
{
    public partial class EmergencyNoticeViewModel : ViewModelBase
    {
        private readonly IEmergencyNoticeApiService _emergencyNoticeService;
        private readonly IMessageApiService _messageService;
        private int _currentDepartmentId = 1; // デフォルト部署ID
        private readonly IWindowManager _windowManager;

        // =================================================================
        // Observable Properties
        // =================================================================

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterNoticeCommand))]
        private bool _isLoading = false;

        [ObservableProperty]
        private Priority _selectedPriority = Priority.Normal;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterNoticeCommand))]
        private string _noticeType = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterNoticeCommand))]
        private string _noticeContent = "";

        [ObservableProperty]
        private ObservableCollection<DepartmentViewModel> _departments = new();

        [ObservableProperty]
        private ObservableCollection<TargetDepartmentViewModel> _selectedTargetDepartments = new();

        [ObservableProperty]
        private ObservableCollection<EmergencyNoticeItemViewModel> _emergencyNotices = new();

        [ObservableProperty]
        private bool _isEmptyList = false;

        // 対象部署選択用
        [ObservableProperty]
        private string _targetDepartmentDisplay = "全部署";

        [ObservableProperty]
        private bool _isAllDepartments = true;

        // =================================================================

        public Priority[] PriorityOptions { get; } = (Priority[])Enum.GetValues(typeof(Priority));

        public EmergencyNoticeViewModel()
        {
            // デザイン時用コンストラクタ
            _emergencyNoticeService = null!;
            _messageService = null!;
            _windowManager = null!;

            // デザインデータ
            var dummyDept1 = new Department { Id = 1, Name = "4病棟" };
            var dummyDept2 = new Department { Id = 2, Name = "3病棟" };
            var dummyDept3 = new Department { Id = 3, Name = "総務課" };
            Departments = new ObservableCollection<DepartmentViewModel>
            {
                new DepartmentViewModel(dummyDept1, AddTargetDepartment),
                new DepartmentViewModel(dummyDept2, AddTargetDepartment),
                new DepartmentViewModel(dummyDept3, AddTargetDepartment)
            };

            SelectedTargetDepartments = new ObservableCollection<TargetDepartmentViewModel>
            {
                new TargetDepartmentViewModel(dummyDept2, RemoveTargetDepartment)
            };

            NoticeType = "C対応";
            NoticeContent = "4病棟でコロナ患者の対応中です";
            SelectedPriority = Priority.High;
            UpdateTargetDepartmentDisplay();

            // 履歴のデザインデータ
            var dummyNotice1 = new EmergencyNotice
            {
                Id = 1,
                Priority = Priority.Urgent,
                NoticeType = "C対応",
                NoticeContent = "3病棟でコロナ患者対応中",
                TargetDepartmentNames = "全部署",
                IsActive = true,
                CreatedAt = DateTime.Now.AddHours(-2)
            };

            var dummyNotice2 = new EmergencyNotice
            {
                Id = 2,
                Priority = Priority.Normal,
                NoticeType = "設備点検",
                NoticeContent = "エレベーター点検のため一時停止",
                TargetDepartmentNames = "4病棟, 3病棟",
                IsActive = false,
                CreatedAt = DateTime.Now.AddDays(-1)
            };

            EmergencyNotices = new ObservableCollection<EmergencyNoticeItemViewModel>
            {
                new EmergencyNoticeItemViewModel(dummyNotice1),
                new EmergencyNoticeItemViewModel(dummyNotice2)
            };
        }

        // ★ 修正：Window参照を受け取るコンストラクター
        public EmergencyNoticeViewModel(
            IEmergencyNoticeApiService emergencyNoticeService,
            IMessageApiService messageService,
            IWindowManager windowManager,
            int departmentId = 1)
        {
            _emergencyNoticeService = emergencyNoticeService;
            _messageService = messageService;
            _currentDepartmentId = departmentId;
            _windowManager = windowManager; ;

            SelectedTargetDepartments.CollectionChanged += (s, e) =>
            {
                RegisterNoticeCommand.NotifyCanExecuteChanged();
                UpdateTargetDepartmentDisplay();
            };

            InitializeAsync();
        }


        private async void InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadDepartmentsAsync();
                await LoadEmergencyNoticesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初期化エラー: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


        // --- Commands ---

        [RelayCommand(CanExecute = nameof(CanRegisterNotice))]
        private async Task RegisterNoticeAsync()
        {
            try
            {
                IsLoading = true;
                var request = new CreateEmergencyNoticeRequest
                {
                    Priority = SelectedPriority,
                    NoticeType = NoticeType,
                    NoticeContent = NoticeContent,
                    TargetDepartments = IsAllDepartments || SelectedTargetDepartments.Count == 0
                        ? null
                        : SelectedTargetDepartments.Select(d => d.Id).OfType<int>().ToList(),
                    CreatedByDepartmentId = _currentDepartmentId
                };

                var response = await _emergencyNoticeService.CreateNoticeAsync(request);
                if (response.Success)
                {
                    ClearForm();
                    await LoadEmergencyNoticesAsync();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanRegisterNotice()
        {
            return !string.IsNullOrWhiteSpace(NoticeType) &&
                   !string.IsNullOrWhiteSpace(NoticeContent) &&
                   !IsLoading;
        }

        [RelayCommand]
        private void AddTargetDepartment(DepartmentViewModel deptVM)
        {
            if (deptVM != null)
            {
                var department = new Department { Id = deptVM.Id, Name = deptVM.Name };
                AddTargetDepartment(department);
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            NoticeType = "";
            NoticeContent = "";
            SelectedPriority = Priority.Normal;
            SelectedTargetDepartments.Clear();
            //EditingNotice = null;
            //IsEditMode = false;
            IsAllDepartments = true;
            UpdateTargetDepartmentDisplay();


        }

        [RelayCommand]
        private void CancelEdit()
        {
            ClearForm();
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            await LoadEmergencyNoticesAsync();
        }

        [RelayCommand]
        private void CloseWindow()
        {
            // ★★★ 変更点：WindowManager経由で自身を閉じる ★★★
            _windowManager.CloseWindow(this);
        }

        [RelayCommand]
        private void CopyToForm(EmergencyNoticeItemViewModel noticeVM)
        {
            if (noticeVM == null) return;

            // フォームにデータをコピー（登録はしない）
            NoticeType = noticeVM.NoticeType;
            NoticeContent = noticeVM.NoticeContent;
            SelectedPriority = noticeVM.Priority;

            // 対象部署もコピー
            SelectedTargetDepartments.Clear();
            if (noticeVM.TargetDepartments?.Count > 0)
            {
                IsAllDepartments = false;
                foreach (var deptId in noticeVM.TargetDepartments)
                {
                    var dept = Departments.FirstOrDefault(d => d.Id == deptId);
                    if (dept != null)
                    {
                        SelectedTargetDepartments.Add(new TargetDepartmentViewModel(
                            new Department { Id = dept.Id, Name = dept.Name },
                            RemoveTargetDepartment));
                    }
                }
            }
            else
            {
                IsAllDepartments = true;
            }

            //IsEditMode = false;
            //EditingNotice = null;

        }

/*    
                [RelayCommand]
                private void EditNotice(EmergencyNoticeItemViewModel noticeVM)
                {
                    if (noticeVM == null) return;

                    // 他のレコードの編集状態をリセット
                    foreach (var notice in EmergencyNotices)
                    {
                        notice.IsBeingEdited = false;
                    }

                    // 編集モードに設定
                    CopyToForm(noticeVM);
                    IsEditMode = true;
                    EditingNotice = noticeVM;

                    // 編集中のレコードを強調
                    noticeVM.IsBeingEdited = true;
                }
 */


        [RelayCommand]
        private async Task ToggleNoticeAsync(EmergencyNoticeItemViewModel noticeVM)
        {
            if (noticeVM == null　|| noticeVM.IsActive == false) return;

            try
            {
                var request = new ToggleEmergencyNoticeRequest
                {
                    Id = noticeVM.Id,
                    IsActive = false
                };

                var response = await _emergencyNoticeService.ToggleNoticeAsync(request);
                if (response.Success)
                {
                    await LoadEmergencyNoticesAsync();
                }
                else
                {
                    await ShowMessageAsync("エラー", response.Message);
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("エラー", $"状態変更に失敗しました: {ex.Message}");
            }
        }

        //[RelayCommand]
        private void OnDepartmentSelectionChanged(DepartmentViewModel? selectedDept)
        {
            if (selectedDept != null)
            {
                AddTargetDepartment(new Department { Id = selectedDept.Id, Name = selectedDept.Name });
            }
        }

        private IRelayCommand<DepartmentViewModel?>? _onDepartmentSelectionChangedCommand;

        public IRelayCommand<DepartmentViewModel?> OnDepartmentSelectionChangedCommand =>
            _onDepartmentSelectionChangedCommand ??= new RelayCommand<DepartmentViewModel?>(OnDepartmentSelectionChanged);


        // --- Private Methods ---

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var response = await _messageService.GetDepartmentsAsync();
                if (response.Success)
                {
                    Departments.Clear();
                    foreach (var dept in response.Departments)
                    {
                        Departments.Add(new DepartmentViewModel(dept, AddTargetDepartment));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"部署リスト読み込みエラー: {ex.Message}");
            }
        }

        private async Task LoadEmergencyNoticesAsync()
        {
            try
            {
                var response = await _emergencyNoticeService.GetAllNoticesAsync();
                if (response.Success)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        EmergencyNotices.Clear();

                        // ソート: 有効→無効の順、その後は作成日時の降順
                        var sortedNotices = response.Notices
                            .OrderByDescending(n => n.IsActive) // 有効が先
                            .ThenByDescending(n => n.CreatedAt) // 作成日時の降順
                            .ToList();

                        foreach (var notice in sortedNotices)
                        {
                            EmergencyNotices.Add(new EmergencyNoticeItemViewModel(notice));
                        }
                        IsEmptyList = EmergencyNotices.Count == 0;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"緊急連絡事項読み込みエラー: {ex.Message}");
            }
        }

        private void AddTargetDepartment(Department department)
        {
            if (department != null && !SelectedTargetDepartments.Any(d => d.Id == department.Id))
            {
                SelectedTargetDepartments.Add(new TargetDepartmentViewModel(department, RemoveTargetDepartment));
                IsAllDepartments = false;
            }
        }

        private void RemoveTargetDepartment(TargetDepartmentViewModel targetDepartmentVM)
        {
            if (targetDepartmentVM != null)
            {
                SelectedTargetDepartments.Remove(targetDepartmentVM);

                // すべての部署が削除されたら全部署に戻す
                if (SelectedTargetDepartments.Count == 0)
                {
                    IsAllDepartments = true;
                }
            }
        }

        private void UpdateTargetDepartmentDisplay()
        {
            if (IsAllDepartments || SelectedTargetDepartments.Count == 0)
            {
                TargetDepartmentDisplay = "全部署";
            }
            else
            {
                TargetDepartmentDisplay = $"選択済み ({SelectedTargetDepartments.Count}部署)";
            }
        }

        private async Task<bool> ShowConfirmDialogAsync(string title, string message)
        {
            // 簡単な確認ダイアログの実装
            // 実際の実装では適切なダイアログシステムを使用
            return true; // 簡略化
        }

        private Task ShowMessageAsync(string title, string message)
        {
            // メッセージダイアログの実装
            Console.WriteLine($"{title}: {message}");
            return Task.CompletedTask;
        }
    }

    // =================================================================
    // Helper ViewModels
    // =================================================================

    public partial class TargetDepartmentViewModel : ViewModelBase
    {
        public int? Id { get; }
        public string? Name { get; }

        private readonly Action<TargetDepartmentViewModel> _removeAction;

        public TargetDepartmentViewModel(Department department, Action<TargetDepartmentViewModel> removeAction)
        {
            if (department.Id.HasValue)
            {
                Id = department.Id;
            }
            if (!string.IsNullOrEmpty(department.Name))
            {
                Name = department.Name;
            }
            _removeAction = removeAction;
        }

        [RelayCommand]
        private void RemoveTargetDepartment()
        {
            _removeAction(this);
        }
    }

    public partial class EmergencyNoticeItemViewModel : ViewModelBase
    {
        public int Id { get; }
        public Priority Priority { get; }
        public string NoticeType { get; }
        public string NoticeContent { get; }
        public List<int>? TargetDepartments { get; }
        public bool IsActive { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }

        // 編集中状態
/*
        [ObservableProperty]
        private bool _isBeingEdited = false;
*/

        // 表示用プロパティ
        public string PriorityDisplay => GetPriorityDisplay(Priority);
        public string PriorityIcon => GetPriorityIcon(Priority);
        public string PriorityColor => GetPriorityColor(Priority);
        public string TargetDepartmentNames { get; }
        public string StatusDisplay => IsActive ? "有効" : "終了";
        public string StatusColor => IsActive ? "#28a745" : "#6c757d";
        public string CreatedAtDisplay => CreatedAt.ToString("M/d HH:mm");

        // 有効/無効切り替え用
        public string ActiveButtonText => "有効";
        public string InactiveButtonText => "終了";
        //public string ActiveButtonColor => IsActive ? "#ffc107" : "#f8f9fa"; // 黄色に変更
        public string InactiveButtonColor => !IsActive ? "#f8f9fa" : "#ffc107"; // 黄色に変更
        //public string ActiveButtonTextColor => IsActive ? "Black" : "#6c757d"; // 黄色背景用に黒文字
        public string InactiveButtonTextColor => !IsActive ? "#6c757d" : "Black"; // 黄色背景用に黒文字

        // リストアイテムの背景色
        public string ItemBackgroundColor => IsActive ? "#E3F2FD" : "#f5f5f5";

        public EmergencyNoticeItemViewModel(EmergencyNotice notice)
        {
            Id = notice.Id;
            Priority = notice.Priority;
            NoticeType = notice.NoticeType;
            NoticeContent = notice.NoticeContent;
            TargetDepartments = notice.TargetDepartments;
            IsActive = notice.IsActive;
            CreatedAt = notice.CreatedAt;
            UpdatedAt = notice.UpdatedAt;
            TargetDepartmentNames = notice.TargetDepartmentNames;
        }

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

        private static string GetPriorityColor(Priority priority) => priority switch
        {
            Priority.Urgent => "#dc3545",
            Priority.High => "#ffc107",
            Priority.Normal => "#28a745",
            Priority.Low => "#007bff",
            _ => "#28a745"
        };
    }
}
