using CommunityToolkit.Mvvm.ComponentModel;

namespace keijiban.ViewModels
{
    /// <summary>
    /// アプリケーション内のすべてのViewModelの基底クラス。
    /// CommunityToolkit.MvvmのObservableObjectを継承することで、
    /// プロパティ変更通知の基本的な実装を提供します。
    /// </summary>
    public abstract partial class ViewModelBase : ObservableObject
    {
    }
}
