using OnamDesk.ViewModels;
using System.Windows.Controls;

namespace OnamDesk.Views
{
    /// <summary>
    /// PatientView.xaml etkileşim mantığı
    /// </summary>
    public partial class PatientView : UserControl
    {
        public PatientView()
        {
            InitializeComponent();
            DataContext = new PatientViewModel();

        }
    }
}
