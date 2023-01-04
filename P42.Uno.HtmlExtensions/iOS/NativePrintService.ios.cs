using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WebKit;
using Microsoft.UI.Xaml.Controls;

#if __WASM__
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#elif !HAS_UNO || NET7_0
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif

namespace P42.Uno.HtmlExtensions
{
    // Just in case this doesn't work for MacOS, take a look at:
    // https://stackoverflow.com/questions/33319295/how-does-one-print-all-wkwebview-on-and-offscreen-content-osx-and-ios

    /// <summary>
    /// Web view extensions service.
    /// </summary>
    class NativePrintService : UIPrintInteractionControllerDelegate, INativePrintService
    {
        /// <summary>
        /// Cans the print.
        /// </summary>
        /// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
        public bool IsAvailable => UIPrintInteractionController.PrintingAvailable;


        /// <summary>
        /// Print the specified viewToPrint and jobName.
        /// </summary>
        /// <param name="unoWebView">View to print.</param>
        /// <param name="jobName">Job name.</param>
        public async Task PrintAsync(BaseWebView unoWebView, string jobName)
        {
            await unoWebView.ExecuteScriptAsync("window.print();");

            /*
            //var effectApplied = viewToPrint.Effects.Any(e => e is Forms9Patch.WebViewPrintEffect);
            //var actualSource = viewToPrint.ActualSource() as WebViewSource;
            var printInfo = UIPrintInfo.PrintInfo;
            printInfo.JobName = jobName;
            printInfo.Duplex = UIPrintInfoDuplex.None;
            printInfo.OutputType = UIPrintInfoOutputType.General;

            var printController = UIPrintInteractionController.SharedPrintController;
            printController.ShowsPageRange = true;
            printController.ShowsPaperSelectionForLoadedPapers = true;
            printController.PrintInfo = printInfo;
            printController.Delegate = this;

            if (unoWebView.GetNativeWebView() is Microsoft.UI.Xaml.Controls.NativeWebView wkWebView)
            {
                var html = await wkWebView.EvaluateJavaScriptAsync("document.documentElement.outerHTML") as NSString;
                printController.PrintFormatter = new UIMarkupTextPrintFormatter(html);
                printController.Present(true, (printInteractionController, completed, error) =>
                {
                    System.Diagnostics.Debug.WriteLine(GetType() + ".PrintAsync: PRESENTED completed[" + completed + "] error[" + error + "]");
                });
            }
            */



        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task PrintAsync(Uri uri, string jobName)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            var printInfo = UIPrintInfo.PrintInfo;

            printInfo.JobName = jobName;
            printInfo.Duplex = UIPrintInfoDuplex.None;
            printInfo.OutputType = UIPrintInfoOutputType.General;

            var printController = UIPrintInteractionController.SharedPrintController;
            printController.ShowsPageRange = true;
            printController.ShowsPaperSelectionForLoadedPapers = true;
            printController.PrintInfo = printInfo;
            printController.Delegate = this;

            var web = new HtmlAgilityPack.HtmlWeb();
            var doc = web.Load(uri.AbsoluteUri);
            var html = doc.DocumentNode.OuterHtml;

            printController.PrintFormatter = new UIMarkupTextPrintFormatter(html);

            printController.Present(true, (printInteractionController, completed, error) =>
            {
                System.Diagnostics.Debug.WriteLine(GetType() + ".PrintAsync : PRESENTED completed[" + completed + "] error[" + error + "]");
            });

        }

    }
}
