using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#if __WASM__
using WebView = P42.Uno.HtmlExtensions.WebViewX;
using WebViewNavigationCompletedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationCompletedEventArgs;
using WebViewNavigationFailedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationFailedEventArgs;
#else
using WebView = Windows.UI.Xaml.Controls.WebView;
using WebViewNavigationCompletedEventArgs = Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs;
using WebViewNavigationFailedEventArgs = Windows.UI.Xaml.Controls.WebViewNavigationFailedEventArgs;
#endif

namespace P42.Uno.HtmlExtensions
{
    class NativePrintService : INativePrintService
    {
        internal static Windows.UI.Xaml.Controls.Page RootPage
        {
            get
            {
                var rootFrame = Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                var page = rootFrame?.Content as Windows.UI.Xaml.Controls.Page;
                var panel = page?.Content as Panel;
                var children = panel.Children.ToList();
                return page;
            }
        }

        internal static Windows.UI.Xaml.Controls.Panel RootPanel => RootPage?.Content as Panel;

        public bool IsAvailable()
        {
            var result = WebAssemblyRuntime.InvokeJS("typeof window.print == 'function';");
            return result == "true";
        }

        public async Task PrintAsync(WebView webView, string jobName)
        {
            //var id = webView.GetHtmlAttribute("id");
            //var result = WebAssemblyRuntime.InvokeJS($"UnoPrint_PrintElement('{id}');");
            var result = await webView.InvokeScriptAsync("window.print", null);
            //await Task.CompletedTask;
        }

        public async Task PrintAsync(string html, string jobName)
        {
            var webView = new WebView();
            webView.Opacity = 0.01;
            webView.NavigationCompleted += OnNavigationComplete;
            webView.NavigationFailed += OnNavigationFailed;
            
            RootPanel.Children.Add(webView);

            System.Diagnostics.Debug.WriteLine("NativePrintService.PrintAsync start NavigateToString html: " + html.Substring(0, Math.Min(256, html.Length)));
            var tcs = new TaskCompletionSource<bool>();
            webView.Tag = tcs;
            webView.NavigateToString(html);
            if (await tcs.Task)
                await PrintAsync(webView, jobName);
            RootPanel.Children.Remove(webView);
        }

        static void OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (sender is WebView webView && webView.Tag is TaskCompletionSource<bool> tcs)
            {
                tcs.TrySetResult(false);
                //await P42.Uno.Controls.Toast.CreateAsync("Print Service Error", "WebView failed to navigate to provided string.  Please try again.\n\nWebErrorStatus: " + e.WebErrorStatus);
                return;
            }
            throw new Exception("Cannot locate WebView or TaskCompletionSource for WebView.OnNavigationFailed");
        }

        static void OnNavigationComplete(WebView webView, WebViewNavigationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("NativePrintService.OnNavigationComplete: " + args.Uri);
            if (webView.Tag is TaskCompletionSource<bool> tcs)
            {
                tcs.TrySetResult(true);
                return;
            }
            throw new Exception("Cannot locate TaskCompletionSource for WebView.NavigationToString");
        }
    }
}
