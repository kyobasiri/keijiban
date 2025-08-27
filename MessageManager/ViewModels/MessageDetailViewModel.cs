// ViewModels/MessageDetailViewModel.cs - 修正版
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MessageManager.ViewModels
{
    public class MessageDetailViewModel
    {
        private readonly MessageDetailApiItem _detail;

        // 表示に必要なプロパティを公開
        public int MessageId => _detail.MessageId;
        public string Subject => _detail.Subject;
        public string FromDeptName => _detail.FromDeptName;
        public string PriorityIcon => GetPriorityIcon(_detail.Priority);
        public string PriorityDisplay { get; }
        public string DateDisplay => _detail.CreatedAt.ToString("M/d HH:mm");
        public string DueDateDisplay => _detail.DueDate?.ToString("M/d") ?? "";
        public bool HasDueDate => _detail.DueDate.HasValue;
        public string Content => _detail.Content;
        public bool RequiresAction => _detail.RequiresAction;
        public DateTime? DueDateValue => _detail.DueDate?.DateTime; // ★ 追加
        public Priority Priority => _detail.Priority; // ★ 追加
        public bool IsActionPanelVisible { get; }

        public IRelayCommand ReplyToSenderCommand { get; }
        public IRelayCommand ReplyToAllCommand { get; }
        public IRelayCommand CopyMessageCommand { get; }
        public IRelayCommand<string> UpdateActionStatusCommand { get; } // ★ IRelayCommand<string> に変更

        // ★このViewModelが自身のデータとしてコレクションを持つ
        public ObservableCollection<MessageRecipientItem> MessageRecipients { get; } = new();
        public ObservableCollection<MessageActionItem> MessageActions { get; } = new();

        public MessageDetailViewModel(MessageDetailApiItem detail,
            IRelayCommand replyToSenderCommand,
            IRelayCommand replyToAllCommand,
            IRelayCommand copyMessageCommand,
            IRelayCommand<string> updateActionStatusCommand,
            bool isReceivedMessage)
        {
            _detail = detail;
            ReplyToSenderCommand = replyToSenderCommand;
            ReplyToAllCommand = replyToAllCommand;
            CopyMessageCommand = copyMessageCommand;
            UpdateActionStatusCommand = updateActionStatusCommand;
            IsActionPanelVisible = _detail.RequiresAction && isReceivedMessage;
            PriorityDisplay = GetPriorityDisplay(detail.Priority);


            // APIモデルから表示用モデルへ変換
            foreach (var recipient in detail.Recipients)
            {
                MessageRecipients.Add(new MessageRecipientItem
                {
                    DeptName = recipient.ToDeptName,
                    ReadDisplay = recipient.ReadAt?.ToString("M/d HH:mm") ?? "未読"
                });
            }

            // ★ 修正④: アクション履歴の色情報も含めて変換
            foreach (var action in detail.Actions)
            {
                MessageActions.Add(new MessageActionItem
                {
                    DeptName = action.DeptName,
                    Status = action.ActionStatus,
                    StatusDisplay = GetActionStatusDisplay(action.ActionStatus),
                    Comment = action.ActionComment,
                    ActionDisplay = action.ActionDate?.ToString("M/d HH:mm") ?? "未対応"
                });
            }
        }

        // ヘルパーメソッド (MainViewModelから移植)
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

        private static string GetActionStatusDisplay(ActionStatus status) => status switch
        {
            ActionStatus.Pending => "未対応",
            ActionStatus.InProgress => "対応中",
            ActionStatus.Completed => "完了",
            ActionStatus.Rejected => "却下",
            _ => "未対応"
        };
    }
}
