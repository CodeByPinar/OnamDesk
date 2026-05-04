using PdfSharp.Fonts;
using System.IO;

namespace OnamDesk.Helpers
{
    public class WindowsFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var name = familyName.ToLower();

            if (name.Contains("arial"))
            {
                if (isBold)
                    return new FontResolverInfo("Arial#Bold");

                return new FontResolverInfo("Arial#Regular");
            }

            if (isBold)
                return new FontResolverInfo("SegoeUI#Bold");

            return new FontResolverInfo("SegoeUI#Regular");
        }

        public byte[] GetFont(string faceName)
        {
            var fontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            var fontFile = faceName switch
            {
                "Arial#Bold" => Path.Combine(fontsPath, "arialbd.ttf"),
                "Arial#Regular" => Path.Combine(fontsPath, "arial.ttf"),
                "SegoeUI#Bold" => Path.Combine(fontsPath, "segoeuib.ttf"),
                "SegoeUI#Regular" => Path.Combine(fontsPath, "segoeui.ttf"),
                _ => Path.Combine(fontsPath, "segoeui.ttf")
            };

            return File.ReadAllBytes(fontFile);
        }
    }
}