#if __MACOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Microsoft.UI.Xaml.Controls;
using ObjCRuntime;
using WebKit;

#if __WASM__
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#elif !HAS_UNO || NET7_0_OR_GREATER
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif

namespace P42.Uno.HtmlExtensions
{
    class NativePrintService : INativePrintService
    {
        public bool IsAvailable => false;

        public async Task PrintAsync(BaseWebView webView, string jobName)
        {
            await webView.ExecuteScriptAsync("window.print();");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task PrintAsync(Uri uri, string jobName)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            var printInfo = NSPrintInfo.SharedPrintInfo;
            var printSettings = printInfo.GetPrintSettings();

            /*
            printInfo.JobName = jobName;
            printSettings.DuplexMode = PrintCore.PMDuplexMode.None;
            printInfo.OutputType = UIPrintInfoOutputType.General;

            var printController = UIPrintInteractionController.SharedPrintController;
            printController.ShowsPageRange = true;
            printController.ShowsPaperSelectionForLoadedPapers = true;
            printController.PrintInfo = printInfo;
            printController.Delegate = this;
            */

            var web = new HtmlAgilityPack.HtmlWeb();
            var doc = web.Load(uri.AbsoluteUri);
            var html = doc.DocumentNode.OuterHtml;

            var webView = new UnoWKWebView();
            var tcs = new TaskCompletionSource<bool>();
            webView.NavigationDelegate = new NavigationDelegate(tcs);
            webView.LoadHtmlString(html, null);

            await tcs.Task;

            //var pdf = await webView.CreatePdfAsync(null);

            await webView.EvaluateJavaScriptAsync("windows.print()");

            // var data = webView.DataWithPdfInsideRect()
            /*
            printController.PrintFormatter = new UIMarkupTextPrintFormatter(html);

            printController.Present(true, (printInteractionController, completed, error) =>
            {
                System.Diagnostics.Debug.WriteLine(GetType() + ".PrintAsync : PRESENTED completed[" + completed + "] error[" + error + "]");
            });
            */
        }

        public async Task PrintAsync(string html, string jobName)
        {
            var uri = await html.ToTempFileUriAsync();
            await uri.PrintAsync(jobName);
        }

    }

    class NavigationDelegate : WKNavigationDelegate
    {
        TaskCompletionSource<bool> taskCompletionSource;

        public NavigationDelegate(TaskCompletionSource<bool> tcs)
        {
            taskCompletionSource = tcs;
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            base.DidFailNavigation(webView, navigation, error);
            taskCompletionSource.SetResult(false);
        }

        public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            base.DidFailProvisionalNavigation(webView, navigation, error);
            taskCompletionSource.SetResult(false);
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            base.DidFinishNavigation(webView, navigation);
            taskCompletionSource.SetResult(true);
        }
    }
}
#endif