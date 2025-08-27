// ViewModels/RecipientViewModel.cs
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using System;
using System.Windows.Input;

namespace MessageManager.ViewModels
{
    public class RecipientViewModel
    {
        private readonly Department _model;
        private readonly Action<RecipientViewModel> _removeAction;

        public string? Name => _model.Name;
        public int? Id => _model.Id; // ★★★ この行を追加 ★★★

        public ICommand RemoveRecipientCommand { get; }

        public RecipientViewModel(Department model, Action<RecipientViewModel> removeAction)
        {
            _model = model;
            _removeAction = removeAction;
            RemoveRecipientCommand = new RelayCommand(() => _removeAction(this));
        }
    }
}
