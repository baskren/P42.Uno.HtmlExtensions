using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WebView2 = P42.Uno.HtmlExtensions.WebViewX;
using WebViewNavigationCompletedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationCompletedEventArgs;
using WebViewNavigationFailedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationFailedEventArgs;

namespace P42.Uno.HtmlExtensions
{
    class NativePrintService : INativePrintService
    {
        internal static Microsoft.UI.Xaml.Controls.Page RootPage
        {
            get
            {
                var rootFrame = Window.Current.Content as Microsoft.UI.Xaml.Controls.Frame;
                var page = rootFrame?.Content as Microsoft.UI.Xaml.Controls.Page;
                var panel = page?.Content as Panel;
                var children = panel.Children.ToList();
                return page;
            }
        }

        internal static Microsoft.UI.Xaml.Controls.Panel RootPanel => RootPage?.Content as Panel;

        public bool IsAvailable
        {
            get
            {
                var result = WebAssemblyRuntime.InvokeJS("typeof window.print == 'function';");
                return result == "true";
            }
        }

        public async Task PrintAsync(WebView2 webView, string jobName)
        {
            //var id = webView.GetHtmlAttribute("id");
            //var result = WebAssemblyRuntime.InvokeJS($"UnoPrint_PrintElement('{id}');");
            var result = await webView.ExecuteJavascriptAsync("window.print()");
            //await Task.CompletedTask;
        }

        public async Task PrintAsync(string html, string jobName)
        {
            var webView = new WebView2();
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
            if (sender is WebView2 webView && webView.Tag is TaskCompletionSource<bool> tcs)
            {
                tcs.TrySetResult(false);
                //await P42.Uno.Controls.Toast.CreateAsync("Print Service Error", "WebView failed to navigate to provided string.  Please try again.\n\nWebErrorStatus: " + e.WebErrorStatus);
                return;
            }
            throw new Exception("Cannot locate WebView or TaskCompletionSource for WebView.OnNavigationFailed");
        }

        static void OnNavigationComplete(WebView2 webView, WebViewNavigationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("NativePrintService.OnNavigationComplete: " + args.Uri);
            if (webView.Tag is TaskCompletionSource<bool> tcs)
            {
                tcs.TrySetResult(true);
                return;
            }
            throw new Exception("Cannot locate TaskCompletionSource for WebView.NavigationToString");
        }

        public async Task PrintAsync(Uri uri, string jobName)
        {
            var webView = new WebView2();
            webView.Opacity = 0.01;
            webView.NavigationCompleted += OnNavigationComplete;
            webView.NavigationFailed += OnNavigationFailed;

            RootPanel.Children.Add(webView);

            System.Diagnostics.Debug.WriteLine($"NativePrintService.PrintAsync start NavigateToString uro: [{uri}] ");
            var tcs = new TaskCompletionSource<bool>();
            webView.Tag = tcs;
            webView.Navigate(uri);
            if (await tcs.Task)
                await PrintAsync(webView, jobName);
            RootPanel.Children.Remove(webView);
        }
    }
}
