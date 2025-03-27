using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.UI.Xaml.Controls;
using UIKit;
using WebKit;
using Windows.Storage;

namespace P42.Uno.HtmlExtensions;

internal class NativeToPdfService : UIPrintInteractionControllerDelegate, INativeToPdfService
{
    private const string LocalStorageFolderName = "P42.Uno.HtmlExtensions.ToPdfService";

    public static string FolderPath()
    {
        var tmpPath = Path.GetTempPath();
        if (string.IsNullOrWhiteSpace(tmpPath))
            throw new Exception($"NO VALUE FOUND FOR System.IO.Path.GetTempPath()");
        FileSystem.AssureExists(tmpPath );
        var root = Path.Combine(tmpPath, LocalStorageFolderName);
        FileSystem.AssureExists(root );
        return root;
    }

    static NativeToPdfService()
    {
        var path = FolderPath();
        Directory.Delete(path, true);
    }

    public bool IsAvailable => UIPrintInteractionController.PrintingAvailable && NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0));

    public async Task<ToFileResult> ToPdfAsync(Uri uri, string fileName, PageSize pageSize, PageMargin margin)
    {
        if (!NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            return await Task.FromResult(new ToFileResult("PDF output not available prior to iOS 11"));

        var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
        const string jScript = "var meta = document.createElement('meta'); meta.setAttribute('name', 'viewport'); meta.setAttribute('content', 'width=device-width'); document.getElementsByTagName('head')[0].appendChild(meta);";
        var wkUScript = new WKUserScript((NSString)jScript, WKUserScriptInjectionTime.AtDocumentEnd, true);
        using var wkUController = new WKUserContentController();
        wkUController.AddUserScript(wkUScript);
        var configuration = new WKWebViewConfiguration
        {
            UserContentController = wkUController
        };
        var webView = new WKWebView(new CGRect(0, 0, pageSize.Width, pageSize.Height), configuration)
        {
            UserInteractionEnabled = false,
            BackgroundColor = UIColor.White,
            NavigationDelegate = new WKNavigationCompleteCallback(fileName, pageSize, margin, taskCompletionSource, NavigationCompleteAsync)
        };
        //webView.LoadHtmlString(html, null);
        if (uri.IsFile)
        {
            var path = uri.AbsolutePath;
            var dir = Directory.GetParent(path).FullName;
            var readAccessUri = new Uri(dir, UriKind.Absolute);
            webView.LoadFileUrl(uri, readAccessUri);
        }
        else
        {
            webView.LoadRequest(new NSUrlRequest(uri));
        }
        return await taskCompletionSource.Task;
    }

    public async Task<ToFileResult> ToPdfAsync(WebView2 unoWebView, string fileName, PageSize pageSize, PageMargin margin)
    {
        if (!NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            return await Task.FromResult(new ToFileResult("PDF output not available prior to iOS 11"));

        if (unoWebView.GetNativeWebView() is not NativeWebView wkWebView)
            return await Task.FromResult(new ToFileResult("Could not get NativeWebView for Uno WebView"));
        
        var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
        var webViewState = new WebViewState(wkWebView);
        wkWebView.BackgroundColor = UIColor.White;
        wkWebView.UserInteractionEnabled = false;
        await GenPdfAsync(wkWebView, fileName, pageSize, margin, taskCompletionSource, webViewState);
        return await taskCompletionSource.Task;
    }

    private static async Task NavigationCompleteAsync(WKWebView webView, string filename, PageSize pageSize, PageMargin margin,
        TaskCompletionSource<ToFileResult> taskCompletionSource)
        => GenPdfAsync(webView, filename, pageSize, margin, taskCompletionSource);


    private static async Task GenPdfAsync(WKWebView webView, string filename, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource, WebViewState webViewState = null)
    {
        
        try
        {
            var widthString = await webView.EvaluateJavaScriptAsync("document.documentElement.offsetWidth");
            var width = double.Parse(widthString.ToString());

            var heightString = await webView.EvaluateJavaScriptAsync("document.documentElement.offsetHeight");
            var height = double.Parse(heightString.ToString());

            if (width < 1 || height < 1)
            {
                taskCompletionSource.SetResult(new ToFileResult("WebView has zero width or height"));
                return;
            }

            webView.ClipsToBounds = false;
            webView.ScrollView.ClipsToBounds = false;

            if (webView.CreatePdfFile(webView.ViewPrintFormatter, pageSize, margin) is { } data)
            {
                var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());
                var file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    await stream.WriteAsync(new ReadOnlyMemory<byte>(data.ToArray()));
                }
                taskCompletionSource.SetResult(new ToFileResult(file));
                data.Dispose();
                return;
            }
            taskCompletionSource.SetResult(new ToFileResult("No data returned."));
        }
        catch (Exception e)
        {
            taskCompletionSource.SetResult(new ToFileResult($"Exception: {e.Message}{(e.InnerException != null
                ? $"Inner exception: {e.InnerException.Message}"
                : null)}"));
        }
        finally
        {
            webViewState?.Reset();
        }

    }

}

internal class WebViewState
{
    private UIColor backgroundColor;
    private bool userInteractionEnabled;
    private bool clipsToBounds;
    private bool scrollViewClipsToBounds;
    private CGRect bounds;
    private WKWebView wkWebView;
    
    public WebViewState(WKWebView webView)
    {
        wkWebView = webView;
        backgroundColor = webView.BackgroundColor;
        userInteractionEnabled = webView.UserInteractionEnabled;
        clipsToBounds = webView.ClipsToBounds;
        scrollViewClipsToBounds = webView.ScrollView.ClipsToBounds;
        bounds = webView.Bounds;
    }

    public void Reset()
    {
        wkWebView.BackgroundColor = backgroundColor;
        wkWebView.UserInteractionEnabled = userInteractionEnabled;
        wkWebView.ClipsToBounds = clipsToBounds;
        wkWebView.ScrollView.ClipsToBounds = scrollViewClipsToBounds;
        wkWebView.Bounds = bounds;
    }
}

internal class PdfRenderer : UIPrintPageRenderer
{
    public NSMutableData PrintToPdf()
    {
        var pdfData = new NSMutableData();
        UIGraphics.BeginPDFContext(pdfData, PaperRect, null);
        PrepareForDrawingPages(new NSRange(0, NumberOfPages));
        var rect = UIGraphics.PDFContextBounds;
        for (var i = 0; i < NumberOfPages; i++)
        {
            UIGraphics.BeginPDFPage();
            DrawPage(i, rect);
        }
        UIGraphics.EndPDFContext();
        return pdfData;
    }
}

internal static class WKWebViewExtensions
{
    public static NSMutableData CreatePdfFile(this WKWebView webView, UIViewPrintFormatter printFormatter, PageSize pageSize, PageMargin margin)
    {
        webView.Bounds = new CGRect(0, 0, (nfloat)pageSize.Width, (nfloat)pageSize.Height);
        margin ??= new PageMargin();
        var pdfPageFrame = new CGRect((nfloat)margin.Left, (nfloat)margin.Top, webView.Bounds.Width - margin.HorizontalThickness, webView.Bounds.Height - margin.VerticalThickness);
        using var renderer = new PdfRenderer();
        renderer.AddPrintFormatter(printFormatter, 0);
        using var k1 = new NSString("paperRect");
        renderer.SetValueForKey(NSValue.FromCGRect(webView.Bounds), k1);
        using var k2 = new NSString("printableRect");
        renderer.SetValueForKey(NSValue.FromCGRect(pdfPageFrame), k2);
        var result = renderer.PrintToPdf();
        renderer.Dispose();
        return result;
    }

}
