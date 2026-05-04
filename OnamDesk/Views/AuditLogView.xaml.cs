using OnamDesk.ViewModels;
using System.Windows.Controls;

namespace OnamDesk.Views
{
    /// <summary>
    /// AuditLogView.xaml etkileşim mantığı
    /// </summary>
    public partial class AuditLogView : UserControl
    {
        public AuditLogView()
        {
            InitializeComponent();
            DataContext = new AuditLogViewModel();
        }
    }
}
