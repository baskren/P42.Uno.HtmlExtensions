using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P42.Uno.HtmlExtensions
{
    public static partial class WebViewExtensions
    {
        public static async Task<string> GetSourceAsHtmlAsync(this Windows.UI.Xaml.Controls.WebView unoWebView)
        {
            var result = await unoWebView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML" });
            return result;
        }

    }
}
