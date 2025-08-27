// ViewModels/MessageListItemViewModel.cs - 完全版
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace MessageManager.ViewModels
{
    public partial class MessageListItemViewModel : ObservableObject
    {
        private readonly MessageListItem _model;
        private readonly Func<MessageListItemViewModel, Task> _selectAction;

        // 表示に必要なプロパティをすべて公開
        public int MessageId => _model.MessageId;
        public string Subject => _model.Subject;
        public string FromDeptName => _model.FromDeptName;
        public string PriorityIcon => _model.PriorityIcon;
        public string DateDisplay => _model.DateDisplay;
        public string DueDateDisplay => _model.DueDateDisplay;
        public bool IsRead => _model.IsRead;
        public bool HasDueDate => _model.HasDueDate;
        public string StatusDisplay => _model.StatusDisplay;
        public string StatusColor => _model.StatusColor;
        public MessageListItem Model => _model; // 元のモデルも公開

        // アクション状況のプロパティ
        public bool RequiresAction => _model.RequiresAction;
        public string ActionStatusDisplay => _model.ActionStatusDisplay;
        public string ActionStatusColor => _model.ActionStatusColor;

        // ★ 新規追加: 選択状態
        [ObservableProperty]
        private bool _isSelected = false;

        public ICommand SelectItemCommand { get; }

        public MessageListItemViewModel(MessageListItem model, Func<MessageListItemViewModel, Task> selectAction)
        {
            _model = model;
            _selectAction = selectAction;
            SelectItemCommand = new AsyncRelayCommand(() => _selectAction(this));
        }
    }
}
