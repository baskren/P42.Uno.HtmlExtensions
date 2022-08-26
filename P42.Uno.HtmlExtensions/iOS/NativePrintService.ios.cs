using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WebKit;
using Microsoft.UI.Xaml.Controls;

namespace P42.Uno.HtmlExtensions
{
    // Just in case this doesn't work for MacOS, take a look at:
    // https://stackoverflow.com/questions/33319295/how-does-one-print-all-wkwebview-on-and-offscreen-content-osx-and-ios

    /// <summary>
    /// Web view extensions service.
    /// </summary>
    public class NativePrintService : UIPrintInteractionControllerDelegate, INativePrintService
    {

        /// <summary>
        /// Print the specified viewToPrint and jobName.
        /// </summary>
        /// <param name="unoWebView">View to print.</param>
        /// <param name="jobName">Job name.</param>
        public async Task PrintAsync(WebView2 unoWebView, string jobName)
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

        /// <summary>
        /// Cans the print.
        /// </summary>
        /// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
        public bool IsAvailable()
        {
            return UIPrintInteractionController.PrintingAvailable;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task PrintAsync(string html, string jobName)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            if (!string.IsNullOrWhiteSpace(html))
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
                printController.PrintFormatter = new UIMarkupTextPrintFormatter(html);

                printController.Present(true, (printInteractionController, completed, error) =>
                {
                    System.Diagnostics.Debug.WriteLine(GetType() + ".PrintAsync : PRESENTED completed[" + completed + "] error[" + error + "]");
                });

            }
        }

    }
}
