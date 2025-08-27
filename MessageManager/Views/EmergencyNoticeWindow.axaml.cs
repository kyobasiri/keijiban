// Views/EmergencyNoticeWindow.axaml.cs (修正後)
using Avalonia.Controls;


namespace MessageManager.Views
{
    public partial class EmergencyNoticeWindow : Window
    {
        public EmergencyNoticeWindow()
        {
            InitializeComponent();
            // DataContextはWindowManagerによって設定されるため、
            // ここでのViewModelの生成ロジックはすべて不要になります。

        }
    }
}
