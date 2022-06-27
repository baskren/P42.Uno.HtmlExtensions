using System;
using WebView = P42.Uno.HtmlExtensions.WebViewX;

namespace P42.Uno.HtmlExtensions
{
    public static class WebViewXExtensions
    {
        internal static string InjectWebBridge(string text)
        {
            var script = "<script src='" +
                NativeWebView.WebViewBridgeScriptUrl +
                "'></script>";
            bool edited = false;
            var index = text.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (index > -1)
            {
                text = text.Insert(index, script);
                edited = true;
            }
            if (!edited)
            {
                index = text.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                if (index > -1)
                {
                    text = text.Insert(index, script);
                    edited = true;
                }
            }
            if (!edited)
            {
                text = script + text;
            }
            //System.Diagnostics.Debug.WriteLine("WebViewXExtensions. new text: " + text);
            return text;
        }
    }
}