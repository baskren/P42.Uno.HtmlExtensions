using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Microsoft.UI.Xaml.Controls.WebView extensions
    /// </summary>
    public static partial class WebViewExtensions
    {
        /// <summary>
        /// Returns the WebView's current Source value as an HTML string
        /// </summary>
        /// <param name="unoWebView"></param>
        /// <returns></returns>
        public static async Task<string> GetSourceAsHtmlAsync(this BaseWebView unoWebView)
        {
            var result = await unoWebView.ExecuteScriptAsync( "document.documentElement.outerHTML" );
            return result;
        }

    }
}
