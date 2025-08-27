// Views/DepartmentMaintenanceWindow.axaml.cs (修正後)
using Avalonia.Controls;

namespace MessageManager.Views
{
    public partial class DepartmentMaintenanceWindow : Window
    {
        public DepartmentMaintenanceWindow()
        {
            InitializeComponent();
            // DataContextはWindowManagerによって設定されるため、
            // ここでのViewModelの生成ロジックはすべて不要になります。

        }
        // メッセージを受信登録

    }
}
