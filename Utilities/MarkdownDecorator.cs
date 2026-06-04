using System.Windows;
using System.Windows.Media;

namespace Utilities
{
    public static class MarkdownDecorator
    {
        public static string Decorate(string htmlSnippet)
        {
            Color bgColor = Colors.White;
            Color fgColor = Colors.Black;
            Color linkColor = Colors.Blue;
            string fontFamilyStr = "Segoe UI, Helvetica, Arial, sans-serif";

            if (Application.Current != null)
            {
                if (Application.Current.TryFindResource("WindowBackground") is Color bg)
                {
                    bgColor = bg;
                }
                if (Application.Current.TryFindResource("TextPrimary") is Color fg)
                {
                    fgColor = fg;
                }
                if (Application.Current.TryFindResource("HyperlinkForeground") is Color link)
                {
                    linkColor = link;
                }
                if (Application.Current.TryFindResource("FontFamilyDefault") is FontFamily font)
                {
                    fontFamilyStr = font.Source ?? fontFamilyStr;
                }
            }

            double dpiScale = 1.0;
            if (Application.Current != null)
            {
                try
                {
                    Window window = Application.Current.MainWindow;
                    if (window == null)
                    {
                        foreach (Window w in Application.Current.Windows)
                        {
                            if (w.IsActive)
                            {
                                window = w;
                                break;
                            }
                        }
                    }
                    if (window != null)
                    {
                        var dpi = VisualTreeHelper.GetDpi(window);
                        dpiScale = dpi.DpiScaleX;
                    }
                }
                catch
                {
                    // Fallback to default
                }
            }

            var scaledFontSize = (int)(15 * dpiScale);
            var checkboxScale = 1.3 * dpiScale;
            var bgHex = $"#{bgColor.R:X2}{bgColor.G:X2}{bgColor.B:X2}";
            var fgHex = $"#{fgColor.R:X2}{fgColor.G:X2}{fgColor.B:X2}";
            var linkHex = $"#{linkColor.R:X2}{linkColor.G:X2}{linkColor.B:X2}";

            return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
  <style>
    body {{
        background-color: {bgHex};
        color: {fgHex};
        font-family: {fontFamilyStr};
        font-size: {scaledFontSize}px;
        line-height: 1.6;
        margin: 12px;
        word-wrap: break-word;
    }}
    a {{
        color: {linkHex};
        text-decoration: none;
    }}
    a:hover {{
        text-decoration: underline;
    }}
    input[type=checkbox] {{
        transform: scale({checkboxScale.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)});
        margin-right: 6px;
        vertical-align: middle;
        cursor: pointer;
    }}
    ul, ol {{
        padding-left: 20px;
        margin-top: 8px;
        margin-bottom: 8px;
    }}
    li {{
        margin-bottom: 6px;
    }}
  </style>
</head>
<body>
{htmlSnippet}
</body>
</html>";
        }
    }
}
