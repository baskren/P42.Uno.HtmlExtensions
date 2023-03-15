using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if __P42WASM__ 
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#elif !HAS_UNO || NET7_0
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
#if !NET7_0
            var result = await unoWebView.ExecuteScriptAsync( "document.documentElement.outerHTML" );
            return result;
#else
            return await Task.FromResult(string.Empty);
#endif
        }

    }
}
