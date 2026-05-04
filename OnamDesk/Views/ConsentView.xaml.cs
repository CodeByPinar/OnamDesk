using OnamDesk.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace OnamDesk.Views
{
    public partial class ConsentView : UserControl
    {
        public DrawingAttributes SignatureDrawingAttributes { get; }

        public ConsentView()
        {
            InitializeComponent();

            SignatureDrawingAttributes = new DrawingAttributes
            {
                Color = Colors.Black,
                Width = 2.5,
                Height = 2.5,
                FitToCurve = true
            };

            DataContext = new ConsentViewModel();
        }

        private async void CreateSignedConsentButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ConsentViewModel viewModel)
            {
                return;
            }

            if (SignatureInkCanvas.Strokes.Count == 0)
            {
                viewModel.StatusMessage = "Lütfen hasta imzası alın.";
                return;
            }

            var signatureData = ConvertInkCanvasToBase64();

            await viewModel.CreateConsentWithSignatureAsync(signatureData);

            SignatureInkCanvas.Strokes.Clear();
        }

        private void ClearSignatureButton_Click(object sender, RoutedEventArgs e)
        {
            SignatureInkCanvas.Strokes.Clear();

            if (DataContext is ConsentViewModel viewModel)
            {
                viewModel.StatusMessage = "İmza alanı temizlendi.";
            }
        }

        private string ConvertInkCanvasToBase64()
        {
            using var memoryStream = new MemoryStream();

            SignatureInkCanvas.Strokes.Save(memoryStream);

            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }
}