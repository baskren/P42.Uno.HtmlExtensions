using System;
using System.Threading.Tasks;
#if __WASM__
using WebView2 = P42.Uno.HtmlExtensions.WebViewX;
#else
using WebView2 = Microsoft.UI.Xaml.Controls.WebView2;
#endif

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Print service.
    /// </summary>
    internal interface INativePrintService
    {
        bool IsAvailable { get; }

        Task PrintAsync(WebView2 webView, string jobName);

        Task PrintAsync(Uri uri, string jobName);


    }
}
