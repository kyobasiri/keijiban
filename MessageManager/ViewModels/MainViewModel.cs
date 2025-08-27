// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using MessageManager.Services;
using MessageManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MessageManager.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IMessageApiService _messageService;
        private int? _departmentId;
        private List<Department> _allDepartments = new();

        [ObservableProperty]
        private DepartmentViewModel? _selectedDepartment;

        [ObservableProperty]
        private ObservableCollection<DepartmentViewModel> _departments = new();

        // 子ViewModel
        [ObservableProperty]
        private NewMessageViewModel? _newMessageVM;

        [ObservableProperty]
        private MessageListViewModel? _messageListVM;

        [ObservableProperty]
        private MessageDetailViewModel? _selectedMessage;

        public MainViewModel(IMessageApiService messageService)
        {
            _messageService = messageService;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadDepartmentsAsync();
            var savedDepartmentId = LocalStorageHelper.LoadSelectedDepartment();
            _departmentId = savedDepartmentId ?? 1;
            SelectedDepartment = Departments.FirstOrDefault(d => d.Id == _departmentId) ?? Departments.FirstOrDefault();
        }

        partial void OnSelectedDepartmentChanged(DepartmentViewModel? value)
        {
            if (value != null)
            {
                _departmentId = value.Id;
                LocalStorageHelper.SaveSelectedDepartment(value.Id);

                // 子ViewModelを再生成
                NewMessageVM = new NewMessageViewModel(_messageService, _departmentId, OnMessageSent);
                MessageListVM = new MessageListViewModel(_messageService);
                MessageListVM.MessageSelected += OnMessageSelected; // イベントを購読
                _ = MessageListVM.LoadMessagesAsync(_departmentId);

                SelectedMessage = null;
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            var response = await _messageService.GetDepartmentsAsync();
            if (response.Success)
            {
                _allDepartments = response.Departments;
                Departments.Clear();
                foreach (var dept in response.Departments)
                {
                    // ここでのActionは不要になったのでnullを渡す
                    Departments.Add(new DepartmentViewModel(dept, _ => { }));
                }
            }
        }

        // MessageListViewModelからの通知を受けて、詳細表示を更新
        private void OnMessageSelected(MessageDetailApiItem messageDetail)
        {
            SelectedMessage = new MessageDetailViewModel(messageDetail,
                ReplyToSenderCommand,
                ReplyToAllCommand,
                CopyMessageCommand,
                UpdateActionStatusCommand,
                MessageListVM?.IsReceivedMessagesSelected ?? false);
        }

        // NewMessageViewModelからの通知を受けて、リストを更新
        private async Task OnMessageSent()
        {
            if (MessageListVM is not null)
            {
                await MessageListVM.LoadMessagesAsync(_departmentId);
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteDetailAction))]
        private void ReplyToSender()
        {
            if (SelectedMessage is null || NewMessageVM is null) return;

            var request = new SendMessageRequest
            {
                Subject = $"Re: {SelectedMessage.Subject}",
                Content = $"\n\n--- 元のメッセージ ---\n> {SelectedMessage.Content.Replace("\n", "\n> ")}",
                ToDeptIds = _allDepartments.Where(d => d.Name == SelectedMessage.FromDeptName).Select(d => d.Id ?? 0).ToList()
            };
            NewMessageVM.SetFormContent(request, _allDepartments);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteDetailAction))]
        private void ReplyToAll()
        {
            if (SelectedMessage is null || NewMessageVM is null) return;

            var recipientNames = SelectedMessage.MessageRecipients.Select(r => r.DeptName).ToList();
            recipientNames.Add(SelectedMessage.FromDeptName);
            var myDeptName = Departments.FirstOrDefault(d => d.Id == _departmentId)?.Name;

            var request = new SendMessageRequest
            {
                Subject = $"Re: {SelectedMessage.Subject}",
                Content = $"\n\n--- 元のメッセージ ---\n> {SelectedMessage.Content.Replace("\n", "\n> ")}",
                ToDeptIds = _allDepartments.Where(d => d.Name is not null && recipientNames.Contains(d.Name) && d.Name != myDeptName)
                                           .Select(d => d.Id ?? 0).ToList()
            };
            NewMessageVM.SetFormContent(request, _allDepartments);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteDetailAction))]
        private void CopyMessage()
        {
            if (SelectedMessage is null || NewMessageVM is null) return;

            var request = new SendMessageRequest
            {
                Subject = SelectedMessage.Subject,
                Content = SelectedMessage.Content,
                Priority = SelectedMessage.Priority,
                RequiresAction = SelectedMessage.RequiresAction,
                DueDate = SelectedMessage.HasDueDate ? SelectedMessage.DueDateValue : null,
                ToDeptIds = _allDepartments.Where(d => SelectedMessage.MessageRecipients.Any(r => r.DeptName == d.Name))
                                           .Select(d => d.Id ?? 0).ToList()
            };
            NewMessageVM.SetFormContent(request, _allDepartments);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteDetailAction))]
        private async Task UpdateActionStatus(string statusString)
        {
            if (SelectedMessage is null) return;

            // 文字列からEnumに変換
            if (Enum.TryParse<ActionStatus>(statusString, true, out var status))
            {
                var request = new ActionUpdateRequest
                {
                    MessageId = SelectedMessage.MessageId,
                    Status = status,
                    Comment = $"{SelectedDepartment?.Name}からの回答"
                };

                var response = await _messageService.UpdateActionStatusAsync(request, _departmentId);
                if (response.Success)
                {
                    // 成功したらリストと詳細の両方を更新する
                    if (MessageListVM is not null)
                    {
                        await MessageListVM.LoadMessagesAsync(_departmentId);
                    }
                    // 詳細を再読み込み
                    var detailResponse = await _messageService.GetMessageDetailAsync(request.MessageId, _departmentId);
                    if (detailResponse.Success && detailResponse.MessageDetail is not null)
                    {
                        OnMessageSelected(detailResponse.MessageDetail);
                    }
                }
            }
        }

        private bool CanExecuteDetailAction() => SelectedMessage is not null;
    }
}

