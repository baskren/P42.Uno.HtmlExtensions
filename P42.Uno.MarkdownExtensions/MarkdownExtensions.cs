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
        => WebView2Extensions.Init(application, window);



    public static async Task EnableMarkdownSupportAsync(this WebView2 webView)
    {
        //TODO: Add VirtualHost.LocalFolders.AddDistinct("UnoLib1"); here
        //webView.IsMarkdownVirtualHostMapped(true);
        await WebView2Extensions.EnableProjectContentFolder("P42.Uno.MarkdownExtensions");
        webView.NavigationStarting += OnNavStart;
        webView.CoreWebView2.DownloadStarting += OnDownloadStarting;
        webView.CoreWebView2.LaunchingExternalUriScheme += OnLaunchingExternalUriScheme;
        webView.CoreWebView2.WebResourceRequested += OnWebResourceRequested;
        webView.CoreWebView2.WebResourceResponseReceived += OnWebResourceResponseReceived;
    }

    private static void OnWebResourceResponseReceived(CoreWebView2 sender, CoreWebView2WebResourceResponseReceivedEventArgs args)
    {
        Log.WriteLine(nameof(OnWebResourceResponseReceived));
    }

    private static void OnWebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
    {
        Log.WriteLine(nameof(OnWebResourceRequested));
    }

    private static void OnLaunchingExternalUriScheme(CoreWebView2 sender, CoreWebView2LaunchingExternalUriSchemeEventArgs args)
    {
        Log.WriteLine(nameof(OnLaunchingExternalUriScheme));
    }

    private static void OnDownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        Log.WriteLine(nameof(OnDownloadStarting));
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

            Uri uri;
            try
            {
                uri = new Uri(uriString);
            }
            catch (System.UriFormatException)
            {
                WebView2Extensions.WinUiMainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    sender.NavigateToString(uriString);
                    return;
                });
                return;
            }

            Log.WriteLine($"uri = [{uri}]");
            Log.WriteLine($"\t.Host = [{uri.Host}]");
            Log.WriteLine($"\t.Port = [{uri.Port}]");
            var localPath = uri.LocalPath;
            Log.WriteLine($"\t.LocalPath = [{localPath}]");
            var directory = Path.GetDirectoryName(localPath);
            Log.WriteLine($"\t.Directory = [{directory}]");
            var filename = Path.GetFileName(localPath);
            Log.WriteLine($"\t.Filename = [{filename}]");
            Log.WriteLine($"\t.Query = [{uri.Query}]");

            if (!uri.LocalPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return;
            
            if (!uriString.StartsWith(VirtualHost.HostUrl))
            {
                Log.WriteLine($"uri: [{uriString}] !>>> [{VirtualHost.HostUrl}]");
                return;
            }
                    
            args.Cancel = true;
            
            var newRequest = $"{MarkdownConverterPagePath}?dir={directory}&filename={filename}&query={uri.Query}";
            Log.WriteLine($"newRequest [{newRequest}]");
            Log.WriteLine(" ");
            Log.WriteLine(" ");

            WebView2Extensions.WinUiMainWindow.DispatcherQueue.TryEnqueue(() =>

            //Task.Run(() =>
            {
                // TODO: Add error handling
                //await Task.Delay(500);
                sender.NavigateToProjectContentFileAsync(newRequest);
            });
        }
        catch (Exception ex)
        {
            Log.WriteLine($"MarkdownExtensions.OnNavStart : [{ex}]");
        }
    }
}
