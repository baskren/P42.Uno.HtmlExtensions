using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static async Task<string> GetSourceAsHtmlAsync(this Microsoft.UI.Xaml.Controls.WebView2 unoWebView)
        {
            var result = await unoWebView.ExecuteScriptAsync( "document.documentElement.outerHTML" );
            return result;
        }

    }
}
