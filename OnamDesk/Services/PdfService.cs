using OnamDesk.Helpers;
using OnamDesk.Models;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OnamDesk.Services
{
    public class PdfService
    {
        private readonly SettingsService _settingsService;

        public PdfService()
        {
            _settingsService = new SettingsService();

            if (GlobalFontSettings.FontResolver is null)
            {
                GlobalFontSettings.FontResolver = new WindowsFontResolver();
            }
        }

        public async Task<string> GenerateConsentPdfAsync(ConsentForm consentForm)
        {
            if (consentForm.Patient is null)
            {
                throw new InvalidOperationException("Hasta bilgisi bulunamadı.");
            }

            if (consentForm.Template is null)
            {
                throw new InvalidOperationException("Şablon bilgisi bulunamadı.");
            }

            var settings = await _settingsService.GetSettingsAsync();

            var archiveFolder = string.IsNullOrWhiteSpace(settings.ArchiveFolderPath)
                ? GetDefaultArchiveFolder()
                : settings.ArchiveFolderPath.Trim();

            if (!Directory.Exists(archiveFolder))
            {
                Directory.CreateDirectory(archiveFolder);
            }

            var pdfTitle = string.IsNullOrWhiteSpace(settings.PdfHeaderTitle)
                ? "ONAM FORMU"
                : settings.PdfHeaderTitle.Trim();

            var fileName = CreatePdfFileName(consentForm.Patient.FullName, consentForm.SignedAt);
            var filePath = Path.Combine(archiveFolder, fileName);

            using var document = new PdfDocument();

            document.Info.Title = pdfTitle;
            document.Info.Author = "OnamDesk";
            document.Info.Subject = consentForm.Template.Name;
            document.Info.Keywords = consentForm.SignatureHash;

            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            var normalFont = new XFont("Arial", 10, XFontStyleEx.Regular);
            var smallFont = new XFont("Arial", 8, XFontStyleEx.Regular);

            double margin = 40;
            double y = 40;
            double lineHeight = 18;
            double bottomMargin = 50;

            double contentWidth = page.Width.Point - margin * 2;

            gfx.DrawString(
                pdfTitle,
                titleFont,
                XBrushes.Black,
                new XRect(margin, y, contentWidth, 30),
                XStringFormats.TopCenter);

            y += 50;

            DrawLabelValue(gfx, headerFont, normalFont, "Hasta Adı Soyadı:", consentForm.Patient.FullName, margin, ref y, lineHeight);
            DrawLabelValue(gfx, headerFont, normalFont, "Doğum Tarihi:", consentForm.Patient.BirthDate.ToString("dd.MM.yyyy"), margin, ref y, lineHeight);
            DrawLabelValue(gfx, headerFont, normalFont, "Doktor:", consentForm.DoctorName, margin, ref y, lineHeight);
            DrawLabelValue(gfx, headerFont, normalFont, "İmzalanma Tarihi:", consentForm.SignedAt.ToString("dd.MM.yyyy HH:mm:ss"), margin, ref y, lineHeight);
            DrawLabelValue(gfx, headerFont, normalFont, "Şablon:", consentForm.Template.Name, margin, ref y, lineHeight);
            DrawLabelValue(gfx, headerFont, normalFont, "Kategori:", consentForm.Template.Category, margin, ref y, lineHeight);

            y += 18;

            EnsureSpace(document, ref page, ref gfx, ref y, 60, margin, bottomMargin);

            gfx.DrawString("Açıklama / İçerik", headerFont, XBrushes.Black, margin, y);
            y += 22;

            y = DrawWrappedTextWithPaging(
                document,
                ref page,
                ref gfx,
                consentForm.Template.ContentJson,
                normalFont,
                margin,
                y,
                contentWidth,
                lineHeight,
                bottomMargin);

            y += 18;

            EnsureSpace(document, ref page, ref gfx, ref y, 60, margin, bottomMargin);

            gfx.DrawString("Risk ve Komplikasyonlar", headerFont, XBrushes.Black, margin, y);
            y += 22;

            y = DrawWrappedTextWithPaging(
                document,
                ref page,
                ref gfx,
                consentForm.Template.Risks,
                normalFont,
                margin,
                y,
                contentWidth,
                lineHeight,
                bottomMargin);

            y += 18;

            EnsureSpace(document, ref page, ref gfx, ref y, 60, margin, bottomMargin);

            gfx.DrawString("Notlar", headerFont, XBrushes.Black, margin, y);
            y += 22;

            var notes = string.IsNullOrWhiteSpace(consentForm.Notes)
                ? "Ek not bulunmamaktadır."
                : consentForm.Notes;

            y = DrawWrappedTextWithPaging(
                document,
                ref page,
                ref gfx,
                notes,
                normalFont,
                margin,
                y,
                contentWidth,
                lineHeight,
                bottomMargin);

            y += 24;

            EnsureSpace(document, ref page, ref gfx, ref y, 140, margin, bottomMargin);

            gfx.DrawString("Hasta İmzası", headerFont, XBrushes.Black, margin, y);
            y += 16;

            var signatureImagePath = CreateSignatureImageFile(consentForm.SignatureData);

            if (!string.IsNullOrWhiteSpace(signatureImagePath) && File.Exists(signatureImagePath))
            {
                using var signatureImage = XImage.FromFile(signatureImagePath);

                gfx.DrawRectangle(XPens.LightGray, margin, y, 220, 80);
                gfx.DrawImage(signatureImage, margin + 8, y + 8, 204, 64);

                y += 96;

                try
                {
                    File.Delete(signatureImagePath);
                }
                catch
                {
                    // Geçici imza görseli silinemezse PDF üretimini bozma.
                }
            }
            else
            {
                gfx.DrawRectangle(XPens.LightGray, margin, y, 220, 80);
                gfx.DrawString("İmza görseli oluşturulamadı.", smallFont, XBrushes.Gray, margin + 10, y + 40);

                y += 96;
            }

            y += 10;

            EnsureSpace(document, ref page, ref gfx, ref y, 80, margin, bottomMargin);

            gfx.DrawString("İmza Hash", headerFont, XBrushes.Black, margin, y);
            y += 18;

            y = DrawWrappedTextWithPaging(
                document,
                ref page,
                ref gfx,
                consentForm.SignatureHash,
                smallFont,
                margin,
                y,
                contentWidth,
                12,
                bottomMargin);

            y += 24;

            EnsureSpace(document, ref page, ref gfx, ref y, 30, margin, bottomMargin);

            gfx.DrawString(
                "Bu belge OnamDesk tarafından dijital olarak oluşturulmuştur.",
                smallFont,
                XBrushes.Gray,
                margin,
                y);

            gfx.Dispose();

            document.Save(filePath);

            return filePath;
        }

        public void OpenPdf(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("PDF dosyası bulunamadı.", filePath);
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
        }

        private static void EnsureSpace(
            PdfDocument document,
            ref PdfPage page,
            ref XGraphics gfx,
            ref double y,
            double requiredHeight,
            double margin,
            double bottomMargin)
        {
            if (y + requiredHeight <= page.Height.Point - bottomMargin)
            {
                return;
            }

            AddNewPage(document, ref page, ref gfx, ref y, margin);
        }

        private static void AddNewPage(
            PdfDocument document,
            ref PdfPage page,
            ref XGraphics gfx,
            ref double y,
            double margin)
        {
            gfx.Dispose();

            page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            gfx = XGraphics.FromPdfPage(page);
            y = margin;
        }

        private static double DrawWrappedTextWithPaging(
            PdfDocument document,
            ref PdfPage page,
            ref XGraphics gfx,
            string text,
            XFont font,
            double x,
            double y,
            double maxWidth,
            double lineHeight,
            double bottomMargin)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return y;
            }

            var paragraphs = text.Replace("\r\n", "\n").Split('\n');

            foreach (var paragraph in paragraphs)
            {
                var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var line = string.Empty;

                if (words.Length == 0)
                {
                    y += lineHeight;
                    continue;
                }

                foreach (var word in words)
                {
                    var testLine = string.IsNullOrWhiteSpace(line)
                        ? word
                        : $"{line} {word}";

                    var size = gfx.MeasureString(testLine, font);

                    if (size.Width > maxWidth)
                    {
                        EnsureSpace(document, ref page, ref gfx, ref y, lineHeight, x, bottomMargin);

                        gfx.DrawString(line, font, XBrushes.Black, x, y);
                        y += lineHeight;

                        line = word;
                    }
                    else
                    {
                        line = testLine;
                    }
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    EnsureSpace(document, ref page, ref gfx, ref y, lineHeight, x, bottomMargin);

                    gfx.DrawString(line, font, XBrushes.Black, x, y);
                    y += lineHeight;
                }

                y += 4;
            }

            return y;
        }

        private static string GetDefaultArchiveFolder()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, "OnamDesk", "Archive");
        }

        private static string CreatePdfFileName(string patientFullName, DateTime signedAt)
        {
            var safeName = MakeSafeFileName(patientFullName.Replace(" ", ""));
            var timestamp = signedAt.ToString("yyyyMMdd_HHmmss");

            return $"ONAM_{safeName}_{timestamp}.pdf";
        }

        private static string MakeSafeFileName(string value)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value;
        }

        private static void DrawLabelValue(
            XGraphics gfx,
            XFont labelFont,
            XFont valueFont,
            string label,
            string value,
            double x,
            ref double y,
            double lineHeight)
        {
            gfx.DrawString(label, labelFont, XBrushes.Black, x, y);
            gfx.DrawString(value, valueFont, XBrushes.Black, x + 130, y);

            y += lineHeight;
        }

        private static string CreateSignatureImageFile(string signatureData)
        {
            if (string.IsNullOrWhiteSpace(signatureData))
            {
                return string.Empty;
            }

            try
            {
                var strokeBytes = Convert.FromBase64String(signatureData);

                using var strokeStream = new MemoryStream(strokeBytes);

                var strokes = new StrokeCollection(strokeStream);

                if (strokes.Count == 0)
                {
                    return string.Empty;
                }

                var bounds = strokes.GetBounds();

                var width = Math.Max(320, bounds.Width + 40);
                var height = Math.Max(120, bounds.Height + 40);

                var drawingVisual = new DrawingVisual();

                using (var drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(
                        Brushes.White,
                        null,
                        new Rect(0, 0, width, height));

                    drawingContext.PushTransform(new TranslateTransform(20 - bounds.X, 20 - bounds.Y));
                    strokes.Draw(drawingContext);
                    drawingContext.Pop();
                }

                var renderBitmap = new RenderTargetBitmap(
                    (int)Math.Ceiling(width),
                    (int)Math.Ceiling(height),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                renderBitmap.Render(drawingVisual);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                var tempFilePath = Path.Combine(
                    Path.GetTempPath(),
                    $"onamdesk_signature_{Guid.NewGuid():N}.png");

                using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write);
                encoder.Save(fileStream);

                return tempFilePath;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}