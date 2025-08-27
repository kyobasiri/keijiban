// ===============================
// MessageManager/ViewModels/DepartmentMaintenanceViewModel.cs - 完全版
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
using MessageManager.Services;

namespace MessageManager.ViewModels
{
    public partial class DepartmentMaintenanceViewModel : ViewModelBase
    {
        private readonly IMessageApiService _messageService;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private ObservableCollection<DepartmentMasterItemViewModel> _departmentMasters = new();

        [ObservableProperty]
        private bool _canSaveAll = false;

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private int _changedCount = 0;

        [ObservableProperty]
        private string _lastUpdateTime = "";

        private readonly IWindowManager _windowManager; // ★ 追加

        public DepartmentMaintenanceViewModel()
        {
            // デザイン時用コンストラクタ
            _messageService = null!;
            _windowManager =  null!; // ★ 追加

            // デザイン用データ
            var dummyItem1 = new DepartmentMaster
            {
                Id = 1,
                DepartmentId = 1,
                DepartmentName = "4病棟",
                ExternalSystemName = "CurrentGroupware",
                ExternalDepartmentId = "101",
                ExternalDepartmentName = "4病棟",
                DisplayCase = 3,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30),
                UpdatedAt = DateTime.Now.AddDays(-1)
            };

            var dummyItem2 = new DepartmentMaster
            {
                Id = 2,
                DepartmentId = 2,
                DepartmentName = "3病棟",
                ExternalSystemName = "CurrentGroupware",
                ExternalDepartmentId = "102",
                ExternalDepartmentName = "3病棟",
                DisplayCase = 1,
                IsActive = false,
                CreatedAt = DateTime.Now.AddDays(-25),
                UpdatedAt = DateTime.Now.AddDays(-2)
            };

            DepartmentMasters = new ObservableCollection<DepartmentMasterItemViewModel>
            {
                new DepartmentMasterItemViewModel(dummyItem1, OnItemChanged),
                new DepartmentMasterItemViewModel(dummyItem2, OnItemChanged)
            };

            TotalCount = 2;
            ChangedCount = 0;
            LastUpdateTime = "最終更新: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public DepartmentMaintenanceViewModel(IMessageApiService messageService, IWindowManager windowManager) // ★ 引数変更
        {
            _messageService = messageService;
            _windowManager = windowManager;

            // ObservableCollectionの変更イベントをリッスン
            DepartmentMasters.CollectionChanged += (sender, args) =>
            {
                UpdateChangeCounts();
            };

            InitializeAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDepartmentMastersAsync();
        }


        [RelayCommand]
        private async Task SaveAllChangesAsync()
        {
            try
            {
                var changedItems = DepartmentMasters.Where(item => item.HasChanges).ToList();
                if (!changedItems.Any())
                {
                    await ShowMessageAsync("情報", "保存する変更がありません。");
                    return;
                }

                // 入力チェック
                var validationErrors = new List<string>();
                foreach (var item in changedItems)
                {
                    var validationResult = ValidateDepartmentMaster(item);
                    if (!validationResult.IsValid)
                    {
                        validationErrors.Add($"ID {item.Id}: {validationResult.ErrorMessage}");
                    }
                }

                if (validationErrors.Any())
                {
                    await ShowMessageAsync("入力エラー", string.Join("\n", validationErrors));
                    return;
                }

                IsLoading = true;
                StatusMessage = $"{changedItems.Count}件の変更を保存中...";

                var successCount = 0;
                var errorCount = 0;
                var errorMessages = new List<string>();

                foreach (var item in changedItems)
                {
                    try
                    {
                        var request = new UpdateDepartmentMasterRequest
                        {
                            Id = item.Id,
                            DepartmentId = item.DepartmentId,
                            DepartmentName = item.DepartmentName,
                            DisplayCase = item.DisplayCaseValue,
                            IsActive = item.IsActive
                        };

                        var response = await _messageService.UpdateDepartmentMasterAsync(request);

                        if (response.Success)
                        {
                            item.MarkAsSaved();
                            successCount++;
                        }
                        else
                        {
                            errorCount++;
                            errorMessages.Add($"ID {item.Id}: {response.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errorMessages.Add($"ID {item.Id}: {ex.Message}");
                    }
                }

                // 結果表示
                var resultMessage = $"保存完了\n成功: {successCount}件";
                if (errorCount > 0)
                {
                    resultMessage += $"\nエラー: {errorCount}件\n\n【エラー詳細】\n{string.Join("\n", errorMessages)}";
                }

                await ShowMessageAsync("保存結果", resultMessage);

                // 変更カウント更新
                UpdateChangeCounts();
                LastUpdateTime = "最終更新: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("エラー", $"一括保存中にエラーが発生しました。\n{ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StatusMessage = "";
            }
        }

        [RelayCommand]
        private void CloseWindow()
        {
            // ★★★ 変更点：WindowManager経由で自身を閉じる ★★★
            _windowManager.CloseWindow(this);
        }

        private async void InitializeAsync()
        {
            await LoadDepartmentMastersAsync();
        }

        private async Task LoadDepartmentMastersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "部署マスタを読み込み中...";

                var response = await _messageService.GetAllDepartmentMastersAsync();

                if (response.Success)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        // 一旦新しいコレクションを作成
                        var newCollection = new ObservableCollection<DepartmentMasterItemViewModel>();

                        foreach (var master in response.DepartmentMasters)
                        {
                            newCollection.Add(new DepartmentMasterItemViewModel(master, OnItemChanged));
                        }

                        // 既存のコレクションをクリアして新しいアイテムを追加
                        DepartmentMasters.Clear();
                        foreach (var item in newCollection)
                        {
                            DepartmentMasters.Add(item);
                        }

                        TotalCount = DepartmentMasters.Count;
                        UpdateChangeCounts();
                        LastUpdateTime = "最終更新: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        // プロパティ変更通知を明示的に発火
                        OnPropertyChanged(nameof(DepartmentMasters));
                        OnPropertyChanged(nameof(TotalCount));
                    });
                }
                else
                {
                    await ShowMessageAsync("エラー", $"部署マスタの読み込みに失敗しました。\n{response.Message}");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("エラー", $"部署マスタの読み込み中にエラーが発生しました。\n{ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StatusMessage = "";
            }
        }

        private void OnItemChanged(DepartmentMasterItemViewModel item)
        {
            UpdateChangeCounts();
        }

        private void UpdateChangeCounts()
        {
            ChangedCount = DepartmentMasters.Count(item => item.HasChanges);
            CanSaveAll = ChangedCount > 0;
        }

        
        private ValidationResult ValidateDepartmentMaster(DepartmentMasterItemViewModel item)
        {
            // 重複チェック（外部システム名 + 部署ID の組み合わせ）
            // DepartmentIdがnullでないことを保証した上でチェック
            if (!item.DepartmentId.HasValue)
            {
                return new ValidationResult(true, "");
            }

            var duplicateItem = DepartmentMasters.FirstOrDefault(d =>
                d.Id != item.Id &&
                d.ExternalSystemName == item.ExternalSystemName &&
                d.DepartmentId.HasValue && // nullチェックを追加
                d.DepartmentId.Value == item.DepartmentId.Value);

            if (duplicateItem != null)
            {
                return new ValidationResult(false,
                    $"同じ外部システム名「{item.ExternalSystemName}」で部署ID「{item.DepartmentId.Value}」は既に存在します。");
            }

            return new ValidationResult(true, "");
        }


        private Task ShowMessageAsync(string title, string message)
        {
            try
            {
                    var dialog = new Window
                    {
                        Title = title,
                        Width = 450,
                        Height = 250,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = false,
                        Content = new StackPanel
                        {
                            Margin = new Avalonia.Thickness(20),
                            Children =
                            {
                                new ScrollViewer
                                {
                                    MaxHeight = 150,
                                    Content = new TextBlock
                                    {
                                        Text = message,
                                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                                    }
                                },
                                new Button
                                {
                                    Content = "OK",
                                    Width = 80,
                                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                    Margin = new Avalonia.Thickness(0, 20, 0, 0)
                                }
                            }
                        }
                    };

                    if (dialog.Content is StackPanel panel)
                    {
                        var button = panel.Children.OfType<Button>().First();
                        button.Click += (s, e) => dialog.Close();
                    }
                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dialog display error: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        private class ValidationResult
        {
            public bool IsValid { get; }
            public string ErrorMessage { get; }

            public ValidationResult(bool isValid, string errorMessage)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
            }
        }
    }

    // 部署マスタアイテムViewModel
    public partial class DepartmentMasterItemViewModel : ViewModelBase
    {
        private readonly Action<DepartmentMasterItemViewModel> _changeNotificationAction;
        private readonly DepartmentMaster _originalData;

        [ObservableProperty] private int _id;
        [ObservableProperty] private int? _departmentId;
        [ObservableProperty] private string? _departmentName = "";
        [ObservableProperty] private string _externalSystemName = "";
        [ObservableProperty] private string _externalDepartmentId = "";
        [ObservableProperty] private string _externalDepartmentName = "";
        [ObservableProperty] private byte _displayCaseValue;
        [ObservableProperty] private bool _isActive;
        [ObservableProperty] private DateTime _createdAt;
        [ObservableProperty] private DateTime _updatedAt;
        [ObservableProperty] private bool _hasChanges = false;

        public string CreatedAtDisplay => CreatedAt.ToString("yyyy/MM/dd HH:mm");
        public string UpdatedAtDisplay => UpdatedAt.ToString("yyyy/MM/dd HH:mm");

        // DisplayCase用のオプション
        public List<DisplayCaseOption> DisplayCaseOptions { get; } = new()
        {
            new DisplayCaseOption { Value = 1, Display = "1行目専用" },
            new DisplayCaseOption { Value = 2, Display = "2行目専用" },
            new DisplayCaseOption { Value = 3, Display = "両方" }
        };

        public DisplayCaseOption SelectedDisplayCase
        {
            get => DisplayCaseOptions.First(x => x.Value == DisplayCaseValue);
            set
            {
                if (value != null && DisplayCaseValue != value.Value)
                {
                    DisplayCaseValue = value.Value;
                    OnPropertyChanged();
                    CheckForChanges();
                    _changeNotificationAction(this);
                }
            }
        }

        public DepartmentMasterItemViewModel(DepartmentMaster data, Action<DepartmentMasterItemViewModel> changeNotificationAction)
        {
            _changeNotificationAction = changeNotificationAction;
            _originalData = data;

            Id = data.Id;
            DepartmentId = data.DepartmentId;
            DepartmentName = data.DepartmentName;
            ExternalSystemName = data.ExternalSystemName;
            ExternalDepartmentId = data.ExternalDepartmentId ?? "";
            ExternalDepartmentName = data.ExternalDepartmentName;
            DisplayCaseValue = data.DisplayCase;
            IsActive = data.IsActive;
            CreatedAt = data.CreatedAt;
            UpdatedAt = data.UpdatedAt;
        }

        partial void OnDepartmentIdChanged(int? value)
        {
            CheckForChanges();
            _changeNotificationAction(this);
        }

        partial void OnDepartmentNameChanged(string? value)
        {
            CheckForChanges();
            _changeNotificationAction(this);
        }

        partial void OnDisplayCaseValueChanged(byte value)
        {
            OnPropertyChanged(nameof(SelectedDisplayCase));
            CheckForChanges();
            _changeNotificationAction(this);
        }

        partial void OnIsActiveChanged(bool value)
        {
            CheckForChanges();
            _changeNotificationAction(this);
        }

        public void MarkAsSaved()
        {
            HasChanges = false;
            _originalData.DepartmentId = DepartmentId;
            _originalData.DepartmentName = DepartmentName;
            _originalData.DisplayCase = DisplayCaseValue;
            _originalData.IsActive = IsActive;
            _originalData.UpdatedAt = DateTime.Now;
            UpdatedAt = _originalData.UpdatedAt;
        }

        private void CheckForChanges()
        {
            HasChanges = DepartmentId != _originalData.DepartmentId ||
                        DepartmentName != _originalData.DepartmentName ||
                        DisplayCaseValue != _originalData.DisplayCase ||
                        IsActive != _originalData.IsActive;
        }
    }

    // DisplayCase用のオプションクラス
    public class DisplayCaseOption
    {
        public byte Value { get; set; }
        public string Display { get; set; } = "";

        public override string ToString() => Display;
    }
}
