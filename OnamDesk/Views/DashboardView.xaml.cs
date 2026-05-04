using OnamDesk.ViewModels;
using System.Windows.Controls;

namespace OnamDesk.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            DataContext = new DashboardViewModel();
        }
    }
}