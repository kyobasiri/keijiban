// Services/WindowManager.cs
using Avalonia.Controls;
using MessageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MessageManager.Services
{
    public class WindowManager : IWindowManager
    {
        private readonly IServiceProvider _serviceProvider;
        // ★★★★★ 開いているウィンドウを管理するための辞書 ★★★★★
        private readonly Dictionary<ViewModelBase, Window> _openWindows = new();

        public WindowManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowWindow<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            var viewType = FindViewTypeForViewModel(typeof(TViewModel));

            // 既に同じViewModelのウィンドウが開いている場合は、それをアクティブにする
            if (_openWindows.TryGetValue(viewModel, out var existingWindow))
            {
                existingWindow.Activate();
                return;
            }

            var window = (Window)_serviceProvider.GetRequiredService(viewType);
            window.DataContext = viewModel;

            // ★★★★★ ウィンドウが閉じられた時に辞書から削除するイベントハンドラを登録 ★★★★★
            window.Closed += (sender, args) =>
            {
                _openWindows.Remove(viewModel);
            };

            _openWindows.Add(viewModel, window);
            window.Show();
        }

        // ★★★★★ 追加：ウィンドウを閉じるためのメソッドの実装 ★★★★★
        public void CloseWindow(ViewModelBase viewModel)
        {
            if (_openWindows.TryGetValue(viewModel, out var window))
            {
                window.Close();
                _openWindows.Remove(viewModel);
            }
        }

        private Type FindViewTypeForViewModel(Type viewModelType)
        {
            var viewName = viewModelType.Name.Replace("ViewModel", "Window");
            var viewTypeName = $"MessageManager.Views.{viewName}";
            var viewType = Assembly.GetExecutingAssembly().GetType(viewTypeName);

            if (viewType == null || !typeof(Window).IsAssignableFrom(viewType))
            {
                throw new InvalidOperationException($"Could not find a window of type '{viewTypeName}' for the view model '{viewModelType.Name}'.");
            }
            return viewType;
        }
    }
}
