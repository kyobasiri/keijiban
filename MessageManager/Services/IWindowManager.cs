// Services/IWindowManager.cs

using MessageManager.ViewModels;

namespace MessageManager.Services
{
    public interface IWindowManager
    {
        /// <summary>
        /// 指定されたViewModelに対応するWindowを表示します。
        /// </summary>
        /// <typeparam name="TViewModel">表示したいViewModelの型</typeparam>
        void ShowWindow<TViewModel>() where TViewModel : ViewModelBase;
        void CloseWindow(ViewModelBase viewModel);
    }
}
