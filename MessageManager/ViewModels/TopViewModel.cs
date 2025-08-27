// ViewModels/TopViewModel.cs
using CommunityToolkit.Mvvm.Input;
using MessageManager.Services;
using MessageManager.ViewModels;

namespace MessageManager.ViewModels
{
    public partial class TopViewModel : ViewModelBase
    {
        private readonly IWindowManager _windowManager;

        public TopViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public TopViewModel()
        {
            _windowManager = null!;
        }

        [RelayCommand]
        private void OpenMessageManager()
        {
            _windowManager.ShowWindow<MainViewModel>();
        }

        [RelayCommand]
        private void OpenEmergencyNotice()
        {
            _windowManager.ShowWindow<EmergencyNoticeViewModel>();
        }

        [RelayCommand]
        private void OpenDepartmentMaintenance()
        {
            _windowManager.ShowWindow<DepartmentMaintenanceViewModel>();
        }
    }
}
