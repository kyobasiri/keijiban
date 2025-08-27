// ViewModels/NewMessageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageManager.Models;
using MessageManager.Services;
using MessageManager.ViewModels;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MessageManager.ViewModels { 
public partial class NewMessageViewModel : ViewModelBase
{
    private readonly IMessageApiService _messageService;
    private readonly Func<Task> _onMessageSent;
    private int? _currentDepartmentId;

    [ObservableProperty]
    private ObservableCollection<DepartmentViewModel> _departments = new();

    [ObservableProperty]
    private ObservableCollection<RecipientViewModel> _selectedRecipients = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string _messageSubject = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string _messageContent = "";

    [ObservableProperty]
    private Priority _selectedPriority = Priority.Normal;

    [ObservableProperty]
    private DateTimeOffset? _dueDate = null;

    [ObservableProperty]
    private bool _requiresAction = false;

    public Priority[] PriorityOptions { get; } = (Priority[])Enum.GetValues(typeof(Priority));

    public NewMessageViewModel(IMessageApiService messageService, int? departmentId, Func<Task> onMessageSent)
    {
        _messageService = messageService;
        _currentDepartmentId = departmentId;
        _onMessageSent = onMessageSent;

        SelectedRecipients.CollectionChanged += (s, e) => SendMessageCommand.NotifyCanExecuteChanged();
        LoadDepartmentsAsync();
    }

    private async void LoadDepartmentsAsync()
    {
        var response = await _messageService.GetDepartmentsAsync();
        if (response.Success)
        {
            Departments.Clear();
            foreach (var dept in response.Departments)
            {
                Departments.Add(new DepartmentViewModel(dept, AddRecipient));
            }
        }
    }

    private void AddRecipient(Department department)
    {
        if (department != null && !SelectedRecipients.Any(r => r.Id == department.Id))
        {
            SelectedRecipients.Add(new RecipientViewModel(department, RemoveRecipient));
        }
    }

    private void RemoveRecipient(RecipientViewModel recipientVM)
    {
        if (recipientVM != null)
        {
            SelectedRecipients.Remove(recipientVM);
        }
    }

    private bool CanSendMessage()
    {
        return !string.IsNullOrWhiteSpace(MessageSubject) &&
               !string.IsNullOrWhiteSpace(MessageContent) &&
               SelectedRecipients.Count > 0;
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        var request = new SendMessageRequest
        {
            ToDeptIds = SelectedRecipients.Select(r => r.Id).OfType<int>().ToList(),
            Subject = MessageSubject,
            Content = MessageContent,
            Priority = SelectedPriority,
            DueDate = DueDate?.DateTime,
            RequiresAction = RequiresAction
        };

        var response = await _messageService.SendMessageAsync(request, _currentDepartmentId);
        if (response.Success)
        {
            ClearForm();
            await _onMessageSent.Invoke(); // 親ViewModelに通知
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        MessageSubject = "";
        MessageContent = "";
        SelectedPriority = Priority.Normal;
        DueDate = null;
        RequiresAction = false;
        SelectedRecipients.Clear();
    }

    // 他のViewModelからフォームをセットするための公開メソッド
    public void SetFormContent(SendMessageRequest request, List<Department> allDepartments)
    {
        MessageSubject = request.Subject;
        MessageContent = request.Content;
        SelectedPriority = request.Priority;
        DueDate = request.DueDate.HasValue ? new DateTimeOffset(request.DueDate.Value) : null;
        RequiresAction = request.RequiresAction;

        SelectedRecipients.Clear();
        foreach (var recipientId in request.ToDeptIds)
        {
            var dept = allDepartments.FirstOrDefault(d => d.Id == recipientId);
            if (dept != null)
            {
                AddRecipient(dept);
            }
        }
    }
}
}
