using System;
using System.Threading.Tasks;
#if __WASM__
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#elif WINDOWS
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Print service.
    /// </summary>
    internal interface INativePrintService
    {
        bool IsAvailable { get; }

        Task PrintAsync(BaseWebView webView, string jobName);

        Task PrintAsync(Uri uri, string jobName);

        Task PrintAsync(string html, string jobName);
    }
}
