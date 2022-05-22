using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace P42.Uno.HtmlExtensions
{
    public static partial class WebViewExtensions
    {
#if WINDOWS_UWP
        internal static async Task<SizeI> WebViewContentSizeIAsync(this Windows.UI.Xaml.Controls.WebView webView)
        {
            var size = await webView.WebViewContentSizeAsync();
            //return new Windows.Foundation.Size(sizeI.Width, sizeI.Height);
            return new SizeI((int)size.Width, (int)size.Height);
        }
#endif

        public static async Task<Windows.Foundation.Size> WebViewContentSizeAsync(this Windows.UI.Xaml.Controls.WebView webView, int depth = 0, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
        {
            /*
            if (webView is null)
            {
                await Forms9Patch.Debug.RequestUserHelp(null, "webView is null." );
            }
            */
            System.Diagnostics.Debug.WriteLine("WebViewExtensions. webView Actual=[" + webView.ActualWidth + ", " + webView.ActualHeight + "]");
            System.Diagnostics.Debug.WriteLine("WebViewExtensions. webView Desired=[" + webView.DesiredSize + "]");

            var contentWidth = (int)PageSize.Default.Width;
            var contentHeight = (int)PageSize.Default.Height;

            if (depth > 50)
                return new Windows.Foundation.Size(contentWidth, contentHeight);
            if (depth > 0)
                await Task.Delay(100);

            try
            {

                var widthString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollWidth.toString()" });
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. widthString: " + widthString);
                int.TryParse(widthString, out contentWidth);

                System.Diagnostics.Debug.WriteLine("elementHeight = " + webView.Height);

                //var rect = await webView.InvokeScriptAsync("pizzx", new[] { "document.getElementById( 'rasta' ).clientHeight.toString()" });
                // ask the content its height
                var docScrollHeight = await webView.InvokeScriptAsync("eval", new[] { "document.documentElement.scrollHeight.toString()" });
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.scrollHeight " + docScrollHeight);
                
                var bodyScrollHeight = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.body.scrollHeight " + bodyScrollHeight);
                var clientRectHeight = await webView.InvokeScriptAsync("eval", new[] { "document.documentElement.getBoundingClientRect().height.toString()" });
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.getBoundingClientRect().height " + clientRectHeight);
                var clientHeight = await webView.InvokeScriptAsync("eval", new[] { "document.documentElement.clientHeight.toString()" });
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.clientHeight " + clientHeight);
                var innterHeight = await webView.InvokeScriptAsync("eval", new[] { "self.innerHeight.toString()" });
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. self.innerHeight " + innterHeight);
                var offsetHeight = await webView.InvokeScriptAsync("eval", new[] { "document.body.offsetHeight.toString()" });
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.body.offsetHeight " + offsetHeight);
                
                //var heightString = await webView.InvokeScriptAsync("eval", new[] { "Math.max(document.body.scrollHeight, document.body.offsetHeight, document.documentElement.clientHeight, document.documentElement.scrollHeight ).toString()" });//, document.documentElement.offsetHeight ).toString()" });
                var heightString = docScrollHeight;
                int.TryParse(heightString, out contentHeight);

            }
            catch (Exception e)
            {
                //await Forms9Patch.Debug.RequestUserHelp(e, "line = " + line + ", callerName=["+callerName+"]");
                System.Diagnostics.Debug.WriteLine("UwpWebViewExtensions.WebViewContentSizeAsync FAIL: " + e.Message);
                return await WebViewContentSizeAsync(webView, depth + 1, callerName);
            }
            return new Windows.Foundation.Size(contentWidth, contentHeight);
        }

        public static async Task<string> GetHtml(this Windows.UI.Xaml.Controls.WebView webView)
        {
            var html = await webView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            return html;
        }
    }
}
