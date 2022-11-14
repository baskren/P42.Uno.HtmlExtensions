using System;
using System.Threading.Tasks;

#if __WASM__
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#elif NET6_0_WINDOWS10_0_19041_0
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif

namespace P42.Uno.HtmlExtensions
{
    internal interface INativeToPdfService
    {
        /// <summary>
        /// Is PDF generation available?
        /// </summary>
        bool IsAvailable { get; }


        Task<ToFileResult> ToPdfAsync(Uri uri, string fileName, PageSize pageSize, PageMargin margin);

        Task<ToFileResult> ToPdfAsync(BaseWebView webView, string fileName, PageSize pageSize, PageMargin margin);
    }

}
