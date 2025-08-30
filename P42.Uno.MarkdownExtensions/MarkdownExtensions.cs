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



    public static void EnableMarkdownSupport(this WebView2 webView)
    {
        //TODO: Add VirtualHost.LocalFolders.AddDistinct("UnoLib1"); here
        //webView.IsMarkdownVirtualHostMapped(true);
        WebView2Extensions.EnableProjectContentFolder("P42.Uno.MarkdownExtensions");
        webView.NavigationStarting += OnNavStart;
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

            Uri uri = null;
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
