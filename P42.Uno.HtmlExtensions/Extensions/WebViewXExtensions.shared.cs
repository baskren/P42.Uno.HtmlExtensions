using System;
#if __WASM__
using WebView = P42.Uno.HtmlExtensions.WebViewX;
#else
using WebView = Windows.UI.Xaml.Controls.WebView;
#endif

namespace P42.Uno.HtmlExtensions
{
    public static class WebViewXExtensions
    {
        internal static string InjectWebBridge(string text)
        {
#if __WASM__
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
#endif
            return text;
        }
    }
}