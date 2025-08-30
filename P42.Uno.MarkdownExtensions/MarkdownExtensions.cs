using System;
using System.Web;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using P42.Uno;

#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

public static class MarkdownExtensions
{

    /// <summary>
    /// Required initialization method
    /// </summary>
    /// <param name="application"></param>
    /// <param name="window"></param>
    public static void Init(Application application, Window window)
        => HtmlExtensions.Init(application, window);



    public static void EnableMarkdownSupport(this WebView2 webView)
    {
        //TODO: Add VirtualHost.LocalFolders.AddDistinct("UnoLib1"); here
        //webView.IsMarkdownVirtualHostMapped(true);
        WebView2Extensions.EnableProjectContentFolder("P42.Uno.MarkdownExtensions");
        webView.NavigationStarting += OnNavStart;
    }


    /// <summary>
    /// Is Markdown printing available on this platform?
    /// </summary>
    /// <returns></returns>
    public static bool CanPrint() => WebView2Extensions.CanPrint();

    /// <summary>
    /// Print markdown : may throw printing exceptions
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="markdown"></param>
    /// <param name="token"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task PrintAsync(UIElement element, string markdown, CancellationToken token = default)
    {
        if (element.XamlRoot is null)
            throw new ArgumentNullException($"{nameof(element)}.{nameof(element.XamlRoot)}");

        await DialogExtensions.AuxiliaryWebViewAsyncProcessor<bool>.Create(
            element.XamlRoot, markdown,
            PrintFunction,
            showWebContent: OperatingSystem.IsWindows(),
            hideAfterOnContentLoadedTaskComplete: true,
            cancellationToken: token);
        return;

        static async Task<bool> PrintFunction(WebView2 webView, CancellationToken localToken)
        {
            webView.EnableMarkdownSupport();
            await webView.PrintAsync(localToken);
            return true;
        }

    }

    /// <summary>
    /// Try printing markdown : presents errors in a dialog
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="markdown"></param>
    /// <param name="token"></param>
    /// <returns>true on success</returns>
    public static async Task<bool> TryPrintAsync(UIElement element, string markdown, CancellationToken token = default)
    {
        try
        {
            await PrintAsync(element, markdown, token);
            return true;
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(element.XamlRoot!, "Markdown Print", ex);
            return false;
        }
    }

    /// <summary>
    /// Saves markdown as PDF : may throw exceptions
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="markdown"></param>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task SavePdfAsync(UIElement element, string markdown, PdfOptions? options = null, CancellationToken token = default)
    {
        if (element.XamlRoot is null)
            throw new ArgumentNullException($"{nameof(element)}.{nameof(element.XamlRoot)}");

        var fileName = string.IsNullOrEmpty(options?.Filename)
            ? "document"
            : options.Filename;

        await DialogExtensions.AuxiliaryWebViewAsyncProcessor<bool>.Create(
            element.XamlRoot, 
            EnableMarkdown,
            markdown,
            MakePdfFunction,
            cancellationToken: token);
        return;

        async Task<bool> MakePdfFunction(WebView2 webView, CancellationToken localToken)
        {
            var pdfTask = webView.GeneratePdfAsync(options, localToken);
            await WebView2Extensions.InternalSavePdfAsync(element, pdfTask, fileName, localToken);
            return true;
        }

        async Task EnableMarkdown(WebView2 webView)
        {
            webView.EnableMarkdownSupport();
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Try to save PDF from markdown
    /// </summary>
    /// <param name="element"></param>
    /// <param name="markdown"></param>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<bool> TrySavePdfAsync(UIElement element, string markdown, PdfOptions? options = null,
        CancellationToken token = default)
    {
        try
        {
            await SavePdfAsync(element, markdown, options, token);
            return true;
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(element.XamlRoot!, "MARKDOWN PDF", ex);
            return false;
        }
    }

    /// <summary>
    /// Generate PDF
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="markdown"></param>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <returns>pdf, error</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<(byte[]? pdf, string error)> GeneratePdfAsync(this UIElement element, string markdown, PdfOptions? options = null, CancellationToken token = default)
    {
        if (element.XamlRoot == null)
            throw new ArgumentNullException($"{nameof(element)}.{nameof(element.XamlRoot)}");

        return await DialogExtensions.AuxiliaryWebViewAsyncProcessor<(byte[]? pdf, string error)>.Create(
            element.XamlRoot,
            EnableMarkdown,
            markdown,
            MakePdfFunction,
            cancellationToken: token);

        async Task<(byte[]?, string)> MakePdfFunction(WebView2 webView, CancellationToken localToken)
            => await webView.GeneratePdfAsync(options, localToken);

        async Task EnableMarkdown(WebView2 webView)
        {
            webView.EnableMarkdownSupport();
            await Task.CompletedTask;
        }
    }



    private const string MarkdownConverterPagePath = "/P42.Uno.MarkdownExtensions/MarkdownPage3.html";

    private static void OnNavStart(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        try
        {


            Log.WriteLine($"args.Cancel [{args.Cancel}]");
            Log.WriteLine($"args.IsRedirected [{args.IsRedirected}]");
            Log.WriteLine($"args.IsUserInitiated [{args.IsUserInitiated}]");
            Log.WriteLine($"args.NavigationId [{args.NavigationId}]");
            Log.WriteLine($"args.Uri [{args.Uri}]");

            if (string.IsNullOrWhiteSpace(args.Uri))
                return;

            var uriString = args.Uri;

            if (args.Uri.StartsWith("data:text/html;charset=utf-8;base64,", StringComparison.OrdinalIgnoreCase))
            {
                var base64 = args.Uri[36..];
                Log.WriteLine($"base64: [{base64}]");
                var bytes = Convert.FromBase64String(base64);
                uriString = System.Text.Encoding.UTF8.GetString(bytes);
                Log.WriteLine($"uri: [{uriString}]");
            }

            var uri = new Uri(uriString);
            Log.WriteLine($"uri = [{uri}]");
            Log.WriteLine($"\t.AbsolutePath = [{uri.AbsolutePath}]");
            Log.WriteLine($"\t.LocalPath = [{uri.LocalPath}]");
            Log.WriteLine($"\t.PathAndQuery = [{uri.PathAndQuery}]");

            if (!uri.LocalPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return;

            if (uri.Query is string query && !string.IsNullOrWhiteSpace(query))
            {
                var queryParams = HttpUtility.ParseQueryString(query);
                foreach (var key in queryParams.AllKeys)
                    Log.WriteLine($"QUERY {key}: {queryParams[key]}");
            }


            args.Cancel = true;


            //var requestedSource = sender.Source;
            var requestedSource = uri.PathAndQuery;
            Log.WriteLine($"requestedSource [{requestedSource}]");

            var safePath = System.Web.HttpUtility.UrlEncode(requestedSource);
            Log.WriteLine($"safeRelativePath [{safePath}]");
            var newRequest = $"{MarkdownConverterPagePath}?requestUrl={safePath}";
            Log.WriteLine($"newRequest [{newRequest}]");
            Log.WriteLine(" ");
            Log.WriteLine(" ");

            WebView2Extensions.WinUiMainWindow.DispatcherQueue.TryEnqueue(() =>

            //Task.Run(() =>
            {
                // TODO: Add error handling
                //await Task.Delay(500);
                sender.NavigateToProjectContentFile(newRequest);
            });
        }
        catch (Exception ex)
        {
            Log.WriteLine($"MarkdownExtensions.OnNavStart : [{ex}]");
        }
    }
}
