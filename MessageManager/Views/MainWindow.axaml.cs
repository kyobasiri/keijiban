// Views/MainWindow.axaml.cs (修正後)
using Avalonia.Controls;

namespace MessageManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContextはWindowManagerによって設定されるため、
            // ここでのViewModelの生成ロジックはすべて不要になります。
        }
    }
}
