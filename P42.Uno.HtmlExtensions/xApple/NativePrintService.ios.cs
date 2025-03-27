using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using P42.Serilog.QuickLog;

namespace P42.Uno.HtmlExtensions;


// Just in case this doesn't work for MacOS, take a look at:
// https://stackoverflow.com/questions/33319295/how-does-one-print-all-wkwebview-on-and-offscreen-content-osx-and-ios

/// <summary>
/// Web view extensions service.
/// </summary>
internal class NativePrintService : UIPrintInteractionControllerDelegate, INativePrintService
{
    public bool IsAvailable => UIPrintInteractionController.PrintingAvailable;
    
    public async Task PrintAsync(WebView2 unoWebView, string jobName)
    {
        if (!NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            throw new Exception("PDF output not available prior to iOS 11");

        if (unoWebView.GetNativeWebView() is not NativeWebView wkWebView)
            throw new Exception("Could not get NativeWebView for Uno WebView");

        var formatter = wkWebView.ViewPrintFormatter;
        var printController = UIPrintInteractionController.SharedPrintController;
        var printInfo = UIPrintInfo.FromDictionary(new NSDictionary());
        printInfo.OutputType = UIPrintInfoOutputType.General;
        printInfo.JobName = jobName;
        printController.PrintInfo = printInfo;
        printController.PrintFormatter = formatter;

        var tcs = new TaskCompletionSource<bool>();
        var handler = new PrintCompleteHandler(tcs);
       
        /*
        if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
            printController.PresentFromRectInView(new CGRect(x: 10, y: 10, width: 20, height: 20), wkWebView,
                true,
                handler.Handler);
        else
        */
            printController.Present(true, handler.Handler);
        
        await tcs.Task;
        //var result = tcs.Task.Result;
    }
    
    public async Task PrintAsync(Uri uri, string jobName)
    {

        var printInfo = UIPrintInfo.PrintInfo;

        printInfo.JobName = jobName;
        printInfo.Duplex = UIPrintInfoDuplex.None;
        printInfo.OutputType = UIPrintInfoOutputType.General;

        var printController = UIPrintInteractionController.SharedPrintController;
        printController.ShowsPageRange = true;
        printController.ShowsPaperSelectionForLoadedPapers = true;
        printController.PrintInfo = printInfo;
        //printController.Delegate = this;

        var web = new HtmlAgilityPack.HtmlWeb();
        var doc = web.Load(uri);
        var html = doc.DocumentNode.OuterHtml;

        printInfo.OutputType = UIPrintInfoOutputType.General;
        printInfo.JobName = jobName;
        printController.PrintInfo = printInfo;
        printController.PrintFormatter = new UIMarkupTextPrintFormatter(html);
        
        var tcs = new TaskCompletionSource<bool>();
        var handler = new PrintCompleteHandler(tcs);
        printController.Present(true, handler.Handler);

        await tcs.Task;
        //var result = tcs.Task.Result;
    }

    public async Task PrintAsync(string html, string jobName)
    {
        var uri = await html.ToTempFileUriAsync();
        await uri.PrintAsync(jobName);
    }

    public void AllowFileAccess(WebView2 webView, bool access) { }
    
    public void AllowNetworkLoads(WebView2 webView, bool access) { }
    
}


internal class PrintCompleteHandler(TaskCompletionSource<bool> tcs)
{

    public readonly UIPrintInteractionCompletionHandler Handler = (controller, completed, error) =>
    {
        if (completed)
            tcs.TrySetResult(true);
        else if (error is null || string.IsNullOrWhiteSpace(error.LocalizedDescription))
        {
            QLog.Information($"[PRINT] Cancelled");
            tcs.TrySetResult(false);
        }
        else
        {
            QLog.Error(
                $"[PRINT] Failed: {error.Domain} {error.Code} : {error.LocalizedDescription} : {error.LocalizedFailureReason}");
            tcs.TrySetResult(false);
        }            
    };
}


