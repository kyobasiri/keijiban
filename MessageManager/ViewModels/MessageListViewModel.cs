// ViewModels/MessageListViewModel.cs
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using MessageManager.Services;
using MessageManager.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MessageManager.ViewModels
{
    public partial class MessageListViewModel : ViewModelBase
    {
        private readonly IMessageApiService _messageService;
        private int? _currentDepartmentId;

        [ObservableProperty]
        private ObservableCollection<MessageListItemViewModel> _receivedMessages = new();

        [ObservableProperty]
        private ObservableCollection<SentMessageListItemViewModel> _sentMessages = new();

        [ObservableProperty]
        private bool _isReceivedMessagesSelected = true;

        [ObservableProperty]
        private bool _isSentMessagesSelected = false;

        [ObservableProperty]
        private string _selectedMessagesTitle = "受信メッセージ一覧";

        // イベントの代わりにActionを使用して親に通知
        public event Action<MessageDetailApiItem>? MessageSelected;

        public MessageListViewModel(IMessageApiService messageService)
        {
            _messageService = messageService;
        }

        public async Task LoadMessagesAsync(int? departmentId)
        {
            _currentDepartmentId = departmentId;
            if (IsReceivedMessagesSelected)
            {
                await LoadReceivedMessagesAsync();
            }
            else
            {
                await LoadSentMessagesAsync();
            }
        }

        [RelayCommand]
        private async Task SelectReceivedMessagesAsync()
        {
            IsReceivedMessagesSelected = true;
            IsSentMessagesSelected = false;
            SelectedMessagesTitle = "受信メッセージ一覧";
            await LoadReceivedMessagesAsync();
        }

        [RelayCommand]
        private async Task SelectSentMessagesAsync()
        {
            IsReceivedMessagesSelected = false;
            IsSentMessagesSelected = true;
            SelectedMessagesTitle = "送信メッセージ一覧";
            await LoadSentMessagesAsync();
        }

        private async Task LoadReceivedMessagesAsync()
        {
            var response = await _messageService.GetAllReceivedMessagesAsync(_currentDepartmentId);
            if (response.Success)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ReceivedMessages.Clear();
                    foreach (var apiItem in response.Messages) // ★ apiItem
                    {
                        // ★★★★★ エラーCS1503対策: APIモデルからUIモデルへの変換処理を追加 ★★★★★
                        var uiItem = new MessageListItem
                        {
                            MessageId = apiItem.MessageId,
                            Subject = apiItem.Subject,
                            FromDeptName = apiItem.FromDeptName,
                            Priority = apiItem.Priority,
                            CreatedAt = apiItem.CreatedAt,
                            DueDate = apiItem.DueDate,
                            IsRead = apiItem.IsRead,
                            RequiresAction = apiItem.RequiresAction,
                            StatusDisplay = apiItem.StatusDisplay,
                            StatusColor = apiItem.StatusColor,
                            ActionStatus = apiItem.ActionStatus ?? ActionStatus.Pending,
                            IsDone = apiItem.IsDone
                        };
                        var vm = new MessageListItemViewModel(uiItem, SelectMessageAsync); // ★ uiItem を渡す
                        ReceivedMessages.Add(vm);
                    }
                });
            }
        }

        private async Task LoadSentMessagesAsync()
        {
            var response = await _messageService.GetAllSentMessagesAsync(_currentDepartmentId);
            if (response.Success)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SentMessages.Clear();
                    foreach (var apiItem in response.Messages) // ★ apiItem
                    {
                        // ★★★★★ エラーCS1503対策: APIモデルからUIモデルへの変換処理を追加 ★★★★★
                        var uiItem = new SentMessageListItem
                        {
                            MessageId = apiItem.MessageId,
                            Subject = apiItem.Subject,
                            ToDeptNames = apiItem.ToDeptNames,
                            Priority = apiItem.Priority,
                            CreatedAt = apiItem.CreatedAt,
                            DueDate = apiItem.DueDate,
                            RequiresAction = apiItem.RequiresAction,
                            IsDone = apiItem.IsDone
                        };
                        var vm = new SentMessageListItemViewModel(uiItem, SelectMessageAsync); // ★ uiItem を渡す
                        SentMessages.Add(vm);
                    }
                });
            }
        }

        private async Task SelectMessageAsync(object messageItemViewModel)
        {
            int messageId = 0;
            if (messageItemViewModel is MessageListItemViewModel received)
            {
                messageId = received.MessageId;
                foreach (var item in ReceivedMessages) item.IsSelected = item.MessageId == messageId;
                foreach (var item in SentMessages) item.IsSelected = false;
            }
            else if (messageItemViewModel is SentMessageListItemViewModel sent)
            {
                messageId = sent.MessageId;
                foreach (var item in SentMessages) item.IsSelected = item.MessageId == messageId;
                foreach (var item in ReceivedMessages) item.IsSelected = false;
            }

            if (messageId > 0)
            {
                var response = await _messageService.GetMessageDetailAsync(messageId, _currentDepartmentId);
                if (response.Success && response.MessageDetail != null)
                {
                    MessageSelected?.Invoke(response.MessageDetail);
                }
            }
        }
    }
}
