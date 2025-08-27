// ViewModels/SentMessageListItemViewModel.cs - 完全版
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace MessageManager.ViewModels
{
    public partial class SentMessageListItemViewModel : ObservableObject
    {
        private readonly SentMessageListItem _model;
        private readonly Func<SentMessageListItemViewModel, Task> _selectAction;

        // 表示に必要なプロパティをすべて公開
        public int MessageId => _model.MessageId;
        public string Subject => _model.Subject;
        public string ToDeptNames => _model.ToDeptNames;
        public string PriorityIcon => _model.PriorityIcon;
        public string DateDisplay => _model.DateDisplay;
        public string DueDateDisplay => _model.DueDateDisplay;
        public bool HasDueDate => _model.HasDueDate;
        public bool RequiresAction => _model.RequiresAction;
        public SentMessageListItem Model => _model; // 元のモデルも公開

        // ★ 新規追加: 選択状態
        [ObservableProperty]
        private bool _isSelected = false;

        public ICommand SelectItemCommand { get; }

        public SentMessageListItemViewModel(SentMessageListItem model, Func<SentMessageListItemViewModel, Task> selectAction)
        {
            _model = model;
            _selectAction = selectAction;
            SelectItemCommand = new AsyncRelayCommand(() => _selectAction(this));
        }
    }
}
