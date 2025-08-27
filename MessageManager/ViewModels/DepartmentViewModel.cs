// ===============================
// MessageManager/ViewModels/DepartmentViewModel.cs - 拡張版
// ===============================
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using System;

namespace MessageManager.ViewModels
{
    public partial class DepartmentViewModel : ViewModelBase
    {
        public int? Id { get; }
        public string? Name { get; }

        private readonly Action<Department> _addRecipientAction;
        private readonly Action<Department>? _addTargetDepartmentAction;

        public DepartmentViewModel(Department department, Action<Department> addRecipientAction)
        {
            if (department.Id.HasValue) {
                Id = department.Id;
            }
            if (!string.IsNullOrEmpty(department.Name))
            {
                Name = department.Name;
            }
            _addRecipientAction = addRecipientAction;
        }

        // 緊急連絡事項用のコンストラクタ
        public DepartmentViewModel(Department department, Action<Department> addRecipientAction, Action<Department> addTargetDepartmentAction)
        {
            Id = department.Id;
            Name = department.Name;
            _addRecipientAction = addRecipientAction;
            _addTargetDepartmentAction = addTargetDepartmentAction;
        }

        [RelayCommand]
        private void AddRecipient()
        {
            _addRecipientAction(new Department { Id = Id, Name = Name });
        }

        [RelayCommand]
        private void AddTargetDepartment()
        {
            if (_addTargetDepartmentAction != null)
            {
                _addTargetDepartmentAction(new Department { Id = Id, Name = Name });
            }
            
        }
    }
}
