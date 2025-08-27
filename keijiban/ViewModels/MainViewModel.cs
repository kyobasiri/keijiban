using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using keijiban.Configuration;
using keijiban.Helpers;
using keijiban.Models;
using keijiban.Services;
using keijiban.ViewModels.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace keijiban.ViewModels
{
    /// <summary>
    /// メインウィンドウのデータコンテキストとなるViewModel。
    /// UIの表示状態を管理し、ユーザー操作に対するロジックを実行します。
    /// </summary>
    public partial class MainViewModel : ViewModelBase, IAsyncDisposable
    {
        #region Fields and Dependencies

        // --- DIによって注入されるサービス ---
        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;
        private readonly ISignalRService _signalRService;
        private readonly ILogger<MainViewModel> _logger;

        private readonly object _emergencyNoticeLock = new object();

        // --- タイマー ---
        private DispatcherTimer? _dataRefreshTimer;
        private DispatcherTimer? _clockTimer;

        #endregion

        #region Observable Properties

        // CommunityToolkit.Mvvmの[ObservableProperty]属性を使い、プロパティ定義を簡潔にします。
        // これにより、コンパイラがプロパティ変更通知のコードを自動生成します。

        [ObservableProperty]
        private string _currentYear = string.Empty;
        [ObservableProperty]
        private string _currentDate = string.Empty;
        [ObservableProperty]
        private string _currentTime = string.Empty;

        [ObservableProperty]
        private string _lastSyncTime = "データ未取得";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RefreshAllDataCommand))] // IsLoadingが変わったらコマンドの実行可否を再評価
        private bool _isLoading = false;

        // --- 緊急情報 ---
        [ObservableProperty]
        private bool _isEmergencyActive = false;
        [ObservableProperty]
        private string _headerBackgroundColor = AppConstants.Colors.DefaultHeaderBackground;
        [ObservableProperty]
        private string _emergencyMarqueeText = string.Empty;
        [ObservableProperty]
        private ObservableCollection<EmergencyTextSegment> _emergencyTextSegments = new();

        // --- UIの選択状態 ---
        [ObservableProperty]
        private Department? _selectedScheduleGroup;
        [ObservableProperty]
        private Department? _selectedDepartment;
        [ObservableProperty]
        private InfoType? _selectedInfoType;

        // --- UIにバインドされるデータコレクション ---
        public ObservableCollection<Department> ScheduleGroups { get; } = new();
        public ObservableCollection<Department> Departments { get; } = new();
        public ObservableCollection<InfoType> InfoTypes { get; } = new();
        public ObservableCollection<DateHeader> DateHeaders { get; } = new();
        public ObservableCollection<ScheduleColumn> ExternalScheduleRow1 { get; } = new();
        public ObservableCollection<ScheduleColumn> ExternalScheduleRow2 { get; } = new();
        public ObservableCollection<ScheduleColumn> No3ScheduleRow { get; } = new();
        public ObservableCollection<InformationItem> InformationList { get; } = new();
        public ObservableCollection<MessageDisplayItem> SentMessagesList { get; } = new();
        public ObservableCollection<MessageDisplayItem> ReceivedMessagesList { get; } = new();

        
        #endregion

        #region Constructor

        public MainViewModel(
            IApiService apiService,
            ISettingsService settingsService,
            ISignalRService signalRService,
            ILogger<MainViewModel> logger)
        {
            // DIコンテナから提供されたサービスをフィールドに保存
            _apiService = apiService;
            _settingsService = settingsService;
            _signalRService = signalRService;
            _logger = logger;

            _logger.LogInformation("MainViewModel is being constructed.");

            // SignalRのイベントを購読
            _signalRService.EmergencyNoticeUpdated += OnEmergencyNoticeUpdated;

            // UI表示用の固定リストを初期化
            InitializeInfoTypes();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// ViewModelの非同期初期化処理を実行します。
        /// このメソッドは、Viewがロードされた後に一度だけ呼び出されるべきです。
        /// </summary>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Starting asynchronous initialization of MainViewModel.");
            IsLoading = true;
            try
            {
                // UIタイマーを開始
                StartClockTimer();

                // 部署リストのような、他のデータ取得の前提となるデータを最初に読み込む
                await LoadDepartmentsAsync();

                // 保存された前回の選択状態を復元
                await LoadSavedSettingsAsync();

                // 全データの初回読み込み
                await RefreshAllDataAsync();

                // バックグラウンド処理を開始
                StartDataRefreshTimer();
                await _signalRService.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical error occurred during MainViewModel initialization.");
                // TODO: ユーザーにエラーダイアログを表示するなどの処理
            }
            finally
            {
                IsLoading = false;
                _logger.LogInformation("MainViewModel initialization completed.");
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// データを手動で更新するコマンド。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task RefreshAllDataAsync()
        {
            _logger.LogInformation("RefreshAllDataCommand executed.");
            IsLoading = true;
            try
            {
                // 日付ヘッダーを現在の日に合わせて再生成
                InitializeDateHeaders();

                // 各データソースからの読み込みタスクを並行して実行
                var dataLoadingTasks = new List<Task>
                {
                    LoadScheduleGroupDataAsync(),
                    LoadDataForSelectedDepartmentAsync(),
                    LoadDoctorAbsenceDataAsync(),
                    LoadInformationDataAsync(),
                    LoadMessagesAsync(),
                    LoadEmergencyNoticesAsync()
                };
                await Task.WhenAll(dataLoadingTasks);

                LastSyncTime = $"最終データ更新: {DateTime.Now:yyyy年M月d日 HH:mm}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing all data.");
                LastSyncTime = $"データ更新失敗: {DateTime.Now:HH:mm}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// RefreshAllDataCommandが実行可能かどうかを判定します。
        /// </summary>
        private bool CanRefresh() => !IsLoading;

        #endregion

        #region Property Changed Handlers

        // [ObservableProperty]属性によって自動生成される partial メソッドを使い、
        // プロパティが変更されたときの追加ロジックを記述します。

        async partial void OnSelectedScheduleGroupChanged(Department? value)
        {
            if (value is null) return;
            _logger.LogInformation("Selected schedule group changed to: {GroupName}", value.Name);
            await _settingsService.SaveSelectedGroupIdAsync(value.Id);

            // 関連するデータを再読み込み
            await LoadScheduleGroupDataAsync();
            await LoadEmergencyNoticesAsync();
        }

        async partial void OnSelectedDepartmentChanged(Department? value)
        {
            if (value is null) return;
            _logger.LogInformation("Selected department changed to: {DepartmentName}", value.Name);
            await _settingsService.SaveSelectedDepartmentIdAsync(value.Id);

            // 関連するデータを再読み込み
            await LoadDataForSelectedDepartmentAsync(); // スケジュールor情報
            await LoadMessagesAsync();
            await LoadEmergencyNoticesAsync();
        }


        async partial void OnSelectedInfoTypeChanged(InfoType? value)
        {
            if (value is null) return;
            _logger.LogInformation("Selected info type changed to: {InfoTypeName}", value.Name);
            await _settingsService.SaveSelectedInfoTypeIdAsync(value.Id);

            // 表示するスケジュール内容を切り替え
            await LoadDataForSelectedDepartmentAsync();
        }

        #endregion

        #region Data Loading Methods

        /// <summary>
        /// 部署とスケジュールグループのリストをAPIから取得します。
        /// </summary>
        private async Task LoadDepartmentsAsync()
        {
            _logger.LogDebug("Loading department lists...");
            var response = await _apiService.GetDepartmentListsAsync();

            if (response.Success)
            {
                ScheduleGroups.Clear();
                foreach (var dept in response.SchedulegroupViewDepartments) ScheduleGroups.Add(dept);

                Departments.Clear();
                foreach (var dept in response.ViewDepartments) Departments.Add(dept);

                _logger.LogInformation("Successfully loaded {GroupCount} schedule groups and {DeptCount} departments.", ScheduleGroups.Count, Departments.Count);
            }
            else
            {
                _logger.LogError("Failed to load departments: {Message}", response.Message);
            }
        }

        /// <summary>
        /// ファイルに保存された前回の選択状態を復元します。
        /// </summary>
        private async Task LoadSavedSettingsAsync()
        {
            _logger.LogDebug("Loading saved settings...");
            try
            {
                var savedGroupId = await _settingsService.GetSelectedGroupIdAsync();
                var savedDeptId = await _settingsService.GetSelectedDepartmentIdAsync();
                var savedInfoTypeId = await _settingsService.GetSelectedInfoTypeIdAsync();

                SelectedScheduleGroup = ScheduleGroups.FirstOrDefault(g => g.Id == savedGroupId) ?? ScheduleGroups.FirstOrDefault(g => g.Id == AppConstants.Defaults.ScheduleGroupId) ?? ScheduleGroups.FirstOrDefault();
                SelectedDepartment = Departments.FirstOrDefault(d => d.Id == savedDeptId) ?? Departments.FirstOrDefault();
                SelectedInfoType = InfoTypes.FirstOrDefault(i => i.Id == savedInfoTypeId) ?? InfoTypes.FirstOrDefault(i => i.Id == AppConstants.Defaults.InfoTypeId) ?? InfoTypes.FirstOrDefault();

                _logger.LogInformation("Restored settings - Group: '{Group}', Dept: '{Dept}', Info: '{Info}'", SelectedScheduleGroup?.Name, SelectedDepartment?.Name, SelectedInfoType?.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load saved settings.");
            }
        }

        /// <summary>
        /// 現在選択されている情報種別に応じて、部署スケジュールまたはその他情報を読み込みます。
        /// </summary>
        private async Task LoadDataForSelectedDepartmentAsync()
        {
            if (SelectedInfoType?.Id == AppConstants.InfoTypes.Information)
            {
                await LoadExtraScheduleDataAsync();
            }
            else // スケジュール（デフォルト）
            {
                await LoadDepartmentScheduleDataAsync();
            }
        }

        private async Task LoadScheduleGroupDataAsync()
        {
            if (SelectedScheduleGroup is null) return;
            _logger.LogDebug("Loading schedule data for group: {GroupName}", SelectedScheduleGroup.Name);
            var response = await _apiService.GetScheduleGroupDataAsync(DateTime.Today, SelectedScheduleGroup.Id);
            if (response.Success) ConvertApiDataToScheduleColumns(response.Schedules, ExternalScheduleRow1);
        }

        private async Task LoadDepartmentScheduleDataAsync()
        {
            if (SelectedDepartment is null) return;
            _logger.LogDebug("Loading schedule data for department: {DepartmentName}", SelectedDepartment.Name);
            var response = await _apiService.GetDepartmentScheduleDataAsync(DateTime.Today, SelectedDepartment.Id);
            if (response.Success) ConvertApiDataToScheduleColumns(response.Schedules, ExternalScheduleRow2);
        }

        private async Task LoadExtraScheduleDataAsync()
        {
            if (SelectedDepartment is null) return;
            _logger.LogDebug("Loading extra schedule data for department: {DepartmentName}", SelectedDepartment.Name);
            var response = await _apiService.GetExtraScheduleDataAsync(SelectedDepartment.Id, DateTime.Today);
            if (response.Success) ConvertExtraDataToScheduleColumns(response.ExtraSchedules, ExternalScheduleRow2);
        }

        private async Task LoadDoctorAbsenceDataAsync()
        {
            _logger.LogDebug("Loading doctor absence data...");
            var response = await _apiService.GetDoctorAbsencesAsync(DateTime.Today);
            if (response.Success) ConvertDoctorAbsenceDataToScheduleColumns(response.DoctorSchedules, No3ScheduleRow);
        }

        private async Task LoadInformationDataAsync()
        {
            _logger.LogDebug("Loading information data...");
            var response = await _apiService.GetInformationAsync();
            if (response.Success)
            {
                InformationList.Clear();
                var today = DateTime.Today;
                foreach (var info in response.Information)
                {
                    InformationList.Add(new InformationItem
                    {
                        PostDate = info.PostDate,
                        DateDisplay = info.PostDate.ToString("M/d"),
                        Title = info.Title,
                        Content = info.Content,
                        IsToday = info.PostDate.Date == today
                    });
                }
            }
        }

        private async Task LoadMessagesAsync()
        {
            if (SelectedDepartment?.Id is null) return;
            int departmentId = SelectedDepartment.Id.Value;
            _logger.LogDebug("Loading messages for department ID: {DepartmentId}", departmentId);

            var sentTask = _apiService.GetSentMessagesAsync(departmentId);
            var receivedTask = _apiService.GetReceivedMessagesAsync(departmentId);
            await Task.WhenAll(sentTask, receivedTask);

            var sentResponse = await sentTask;
            if (sentResponse.Success)
            {
                SentMessagesList.Clear();
                foreach (var msg in sentResponse.Messages)
                {
                    // ★★★ 省略されていたマッピング処理を実装 ★★★
                    SentMessagesList.Add(new MessageDisplayItem
                    {
                        MessageId = msg.MessageId,
                        Subject = msg.Subject,
                        ToDeptName = msg.ToDeptNames, // APIモデルとUIモデルでプロパティ名が違う可能性を考慮
                        DateDisplay = msg.DateDisplay,
                        DueDateDisplay = msg.DueDateDisplay,
                        IsRead = msg.IsRead,
                        Priority = msg.Priority,
                        PriorityIcon = msg.PriorityIcon,
                        StatusColor = msg.StatusColor,
                        StatusDisplay = msg.StatusDisplay
                    });
                }
            }

            var receivedResponse = await receivedTask;
            if (receivedResponse.Success)
            {
                ReceivedMessagesList.Clear();
                foreach (var msg in receivedResponse.Messages)
                {
                    // ★★★ 省略されていたマッピング処理を実装 ★★★
                    ReceivedMessagesList.Add(new MessageDisplayItem
                    {
                        MessageId = msg.MessageId,
                        Subject = msg.Subject,
                        FromDeptName = msg.FromDeptName,
                        DateDisplay = msg.DateDisplay,
                        DueDateDisplay = msg.DueDateDisplay,
                        IsRead = msg.IsRead,
                        Priority = msg.Priority,
                        PriorityIcon = msg.PriorityIcon,
                        StatusColor = msg.StatusColor,
                        StatusDisplay = msg.StatusDisplay
                    });
                }
            }
        }

        private async Task LoadEmergencyNoticesAsync()
        {
            _logger.LogDebug("Loading emergency notices...");
            var response = await _apiService.GetCombinedActiveNoticesForDepartmentsAsync(SelectedScheduleGroup?.Id, SelectedDepartment?.Id);
            if (response.Success)
            {
                UpdateEmergencyNoticeStatus(response.Notices);
            }
            else
            {
                _logger.LogWarning("Failed to load emergency notices: {Message}", response.Message);
                UpdateEmergencyNoticeStatus(new List<EmergencyNoticeApiItem>()); // エラー時は緊急情報をクリア
            }
        }

        #endregion

        #region UI Update and Helper Methods

        /// <summary>
        /// SignalRから緊急情報更新イベントを受け取った際の処理。
        /// </summary>
        private void OnEmergencyNoticeUpdated(List<EmergencyNoticeApiItem> activeNotices)
        {
            _logger.LogInformation("Received emergency notice update via SignalR.");
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                // UIスレッドでの処理をtry-catchで囲む
                try
                {
                    UpdateEmergencyNoticeStatus(activeNotices);
                }
                catch (Exception ex)
                {
                    // クラッシュの代わりにエラーをログに記録する
                    _logger.LogCritical(ex, "An unhandled exception occurred while updating emergency notices on the UI thread from a SignalR push.");
                }
            });
        }

        // ...

        /// <summary>
        /// 緊急情報のリストを元に、UIの表示状態を更新します。
        /// </summary>
        private void UpdateEmergencyNoticeStatus(List<EmergencyNoticeApiItem> activeNotices)
        {
            lock (_emergencyNoticeLock)
            {
                // (推奨) 念のため、引数がnullの場合のガード節を追加
                if (activeNotices == null)
                {
                    _logger.LogWarning("UpdateEmergencyNoticeStatus was called with a null list.");
                    activeNotices = new List<EmergencyNoticeApiItem>();
                }
                var validNotices = activeNotices.Where(n => n.IsActive).ToList();
                IsEmergencyActive = validNotices.Any();

                if (IsEmergencyActive)
                {
                    HeaderBackgroundColor = AppConstants.Colors.EmergencyHeaderBackground;
                    EmergencyMarqueeText = string.Join("　◇　", validNotices.Select(n => $"{n.NoticeType}：{n.NoticeContent}"));
                    CreateEmergencyTextSegments(validNotices);
                }
                else
                {
                    HeaderBackgroundColor = AppConstants.Colors.DefaultHeaderBackground;
                    EmergencyMarqueeText = string.Empty;
                    EmergencyTextSegments.Clear();
                }
            }
        }

        private void CreateEmergencyTextSegments(List<EmergencyNoticeApiItem> activeNotices)
        {
            EmergencyTextSegments.Clear();
            foreach (var notice in activeNotices.OrderByDescending(n => n.Priority))
            {
                EmergencyTextSegments.Add(new EmergencyTextSegment
                {
                    Text = $"　◇ {notice.NoticeType}：{notice.NoticeContent}",
                    Color = PriorityConverterHelper.GetEmergencyPriorityColor(notice.Priority)
                });
            }
        }

        private void InitializeInfoTypes()
        {
            InfoTypes.Clear();
            InfoTypes.Add(new InfoType { Id = AppConstants.InfoTypes.Schedule, Name = "スケジュール" });
            InfoTypes.Add(new InfoType { Id = AppConstants.InfoTypes.Information, Name = "情報" });
        }

        private void StartClockTimer()
        {
            _clockTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (s, e) => UpdateDateTime());
            _clockTimer.Start();
        }

        private void StartDataRefreshTimer()
        {
            _dataRefreshTimer = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Background, async (s, e) => await RefreshAllDataAsync());
            _dataRefreshTimer.Start();
        }

        private void UpdateDateTime()
        {
            var now = DateTime.Now;
            CurrentYear = $"{now.Year}年 (令和{now.Year - 2018}年)";
            CurrentDate = $"{now.Month}月{now.Day}日 ({GetJapaneseDayOfWeek(now.DayOfWeek)})";
            CurrentTime = now.ToString("HH:mm");
        }

        private void InitializeDateHeaders()
        {
            DateHeaders.Clear();
            var today = DateTime.Today;
            for (int i = 0; i < 8; i++)
            {
                var date = today.AddDays(i);
                DateHeaders.Add(new DateHeader
                {
                    Date = date,
                    Header = $"{date.Month}/{date.Day}({GetJapaneseDayOfWeek(date.DayOfWeek)})",
                    IsToday = i == 0,
                    IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
                });
            }
        }

        private string GetJapaneseDayOfWeek(DayOfWeek dayOfWeek) => dayOfWeek switch
        {
            DayOfWeek.Sunday => "日",
            DayOfWeek.Monday => "月",
            DayOfWeek.Tuesday => "火",
            DayOfWeek.Wednesday => "水",
            DayOfWeek.Thursday => "木",
            DayOfWeek.Friday => "金",
            DayOfWeek.Saturday => "土",
            _ => ""
        };

        #endregion

        #region Data Conversion Helpers

        /// <summary>
        /// APIから取得したスケジュールリストを、UI表示用のスケジュール列コレクションに変換します。
        /// StartDateからEndDateまでの期間を考慮し、各日にスケジュールを展開します。
        /// </summary>
        private void ConvertApiDataToScheduleColumns(List<ScheduleApiItem> apiData, ObservableCollection<ScheduleColumn> targetCollection)
        {
            // 引数がnullなら何もしない
            if (apiData is null) return;

            // UIコレクションを一度完全にクリアする
            targetCollection.Clear();

            // ★★★ 新しいロジックの核心部分 ★★★
            // 日付をキーとし、その日のスケジュール項目のリストを値とする新しい辞書を作成する
            var schedulesExpandedByDay = new Dictionary<DateTime, List<ScheduleApiItem>>();

            // APIから受け取った各スケジュール項目をループ処理
            foreach (var apiItem in apiData)
            {
                // 各スケジュールのStartDateからEndDateまで、1日ずつループ
                for (var day = apiItem.StartDate.Date; day <= apiItem.EndDate.Date; day = day.AddDays(1))
                {
                    // もし、この日付が辞書のキーとしてまだ存在していなければ、新しい空のリストを作成して追加
                    if (!schedulesExpandedByDay.ContainsKey(day))
                    {
                        schedulesExpandedByDay[day] = new List<ScheduleApiItem>();
                    }

                    // この日付のリストに、現在のスケジュール項目を追加
                    schedulesExpandedByDay[day].Add(apiItem);
                }
            }

            // UIに表示すべき日付のマスターリストである DateHeaders を基準にループする
            foreach (var dateHeader in DateHeaders)
            {
                // 新しいスケジュール列を生成
                var newColumn = new ScheduleColumn
                {
                    Date = dateHeader.Date,
                    IsToday = dateHeader.IsToday,
                    IsWeekend = dateHeader.IsWeekend
                };

                // この日付に対応する展開済みスケジュールが辞書に存在するか確認
                if (schedulesExpandedByDay.TryGetValue(dateHeader.Date, out var scheduleItemsForDay))
                {
                    // データが存在すれば、それをUIモデルに変換して列に追加
                    // StartTimeでソートしてから追加する
                    foreach (var apiItem in scheduleItemsForDay.OrderBy(s => s.StartTime))
                    {
                        newColumn.ScheduleItems.Add(new ScheduleItem
                        {
                            Time = apiItem.StartTime ?? "",
                            Title = apiItem.Title,
                            Location = apiItem.Department ?? ""
                        });
                    }
                }

                // データがあってもなくても、日付の列自体は必ずUIコレクションに追加する
                targetCollection.Add(newColumn);
            }
        }

        /// <summary>
        /// APIから取得したその他スケジュールリストを、UI表示用のスケジュール列コレクションに変換します。
        /// </summary>
        private void ConvertExtraDataToScheduleColumns(List<ExtraScheduleDataApiItem> extraData, ObservableCollection<ScheduleColumn> targetCollection)
        {
            if (extraData is null) return;

            targetCollection.Clear();

            var groupedData = extraData.GroupBy(s => s.ScheduleDate.Date)
                                       .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartTime).ToList());

            foreach (var dateHeader in DateHeaders)
            {
                var newColumn = new ScheduleColumn
                {
                    Date = dateHeader.Date,
                    IsToday = dateHeader.IsToday,
                    IsWeekend = dateHeader.IsWeekend
                };

                if (groupedData.TryGetValue(dateHeader.Date, out var scheduleItemsForDay))
                {
                    foreach (var apiItem in scheduleItemsForDay)
                    {
                        newColumn.ScheduleItems.Add(new ScheduleItem
                        {
                            Time = apiItem.StartTime ?? "",
                            Title = apiItem.Title,
                            Location = apiItem.Category
                        });
                    }
                }
                targetCollection.Add(newColumn);
            }
        }

        /// <summary>
        /// APIから取得した医師不在予定を、UI表示用のスケジュール列コレクションに変換します。
        /// </summary>
        private void ConvertDoctorAbsenceDataToScheduleColumns(Dictionary<string, List<DoctorAbsenceApiItem>> doctorSchedules, ObservableCollection<ScheduleColumn> targetCollection)
        {
            if (doctorSchedules is null) return;

            targetCollection.Clear();

            // 日付文字列をキーとする辞書なので、そのまま利用
            const string DateFormat = "yyyy-MM-dd";

            foreach (var dateHeader in DateHeaders)
            {
                var newColumn = new ScheduleColumn
                {
                    Date = dateHeader.Date,
                    IsToday = dateHeader.IsToday,
                    IsWeekend = dateHeader.IsWeekend
                };

                // この日付ヘッダーの日付を、APIの辞書のキー（"yyyy-MM-dd"形式）に変換
                var apiKey = dateHeader.Date.ToString(DateFormat);

                // このキーでAPIデータが存在するか確認
                if (doctorSchedules.TryGetValue(apiKey, out var absenceItemsForDay))
                {
                    foreach (var apiItem in absenceItemsForDay)
                    {
                        newColumn.ScheduleItems.Add(new ScheduleItem
                        {
                            Time = !string.IsNullOrEmpty(apiItem.MiniDetail) ? apiItem.MiniDetail :  "",
                            Title = apiItem.Detail, // DoctorNameがDetailに含まれていると仮定
                            Location = apiItem.Reason ?? ""
                        });
                    }
                }
                targetCollection.Add(newColumn);
            }
        }

        #endregion

        #region IAsyncDisposable Implementation

        /// <summary>
        /// ViewModelが破棄される際にリソースをクリーンアップします。
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("Disposing MainViewModel resources.");

            // タイマーを停止
            _clockTimer?.Stop();
            _dataRefreshTimer?.Stop();

            // SignalRイベントの購読を解除
            if (_signalRService != null)
            {
                _signalRService.EmergencyNoticeUpdated -= OnEmergencyNoticeUpdated;
                await _signalRService.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
