using OnamDesk.ViewModels;
using System.Windows.Controls;

namespace OnamDesk.Views
{
    /// <summary>
    /// TemplateView.xaml etkileşim mantığı
    /// </summary>
    public partial class TemplateView : UserControl
    {
        public TemplateView()
        {
            InitializeComponent();
            DataContext = new TemplateViewModel();
        }
    }
}
