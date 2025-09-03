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

    public static readonly DependencyProperty HtmlIdProperty = DependencyProperty.RegisterAttached(
        "HtmlId", typeof(string), typeof(MarkdownExtensions), new PropertyMetadata("0"));

    
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

        #if BROWSERWASM
        await WasmExtensions.EnableOnLoadAsync(webView);
        #else
        webView.NavigationStarting += OnNavStart;
        #endif
    }
    
    private const string MarkdownConverterPagePath = "/P42.Uno.MarkdownExtensions/MarkdownPage3.html";

    private static void OnNavStart(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        #if !BROWSERWASM
        args.Cancel = RedirectIfMarkdown(sender, args.Uri ?? string.Empty);
        #endif
    }


    internal static bool RedirectIfMarkdown(WebView2 webView, string uriString)
    {
        try
        {
            Log.WriteLine($"MarkdownExtensions.RedirectIfMarkdown[A] : uriString [{uriString}]");

            if (string.IsNullOrWhiteSpace(uriString))
                return false;

            if (uriString.StartsWith("about:"))
                return false;
            
            if (uriString.StartsWith("data:text/html;charset=utf-8;base64,", StringComparison.OrdinalIgnoreCase))
            {
                var base64 = uriString[36..];
                var bytes = Convert.FromBase64String(base64);
                uriString = System.Text.Encoding.UTF8.GetString(bytes);
                Log.WriteLine($"uri: [{uriString}]");
            }

            Log.WriteLine($"MarkdownExtensions.RedirectIfMarkdown[B] : uriString [{uriString}]");
            
            Log.WriteLine($"MarkdownExtensions.RedirectIfMarkdown[strcmp] : [{uriString == "http://localhost:64526/WebContentX/document.md"}]");
            
            Uri uri;
            try
            {
                uri = new Uri(uriString);
            }
            catch (Exception ex)
            {
                Log.WriteLine($"MarkdownExtensions.RedirectIfMarkdown[EX] : {ex}");
                // TODO: Need to test webview.NavigateToString();
                /*
                WebView2Extensions.WinUiMainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    webView.NavigateToString(uriString);
                });
                return true;*/
                return true;
            }

            Log.WriteLine($"MarkdownExtensions.RedirectIfMarkdown[C] : uriString [{uriString}]");

            //Log.WriteLine($"uri = [{uri}]");
            //Log.WriteLine($"\t.Host = [{uri.Host}]");
            //Log.WriteLine($"\t.Port = [{uri.Port}]");
            var localPath = uri.LocalPath;
            //Log.WriteLine($"\t.LocalPath = [{localPath}]");
            var directory = Path.GetDirectoryName(localPath);
            //Log.WriteLine($"\t.Directory = [{directory}]");
            var filename = Path.GetFileName(localPath);
            //Log.WriteLine($"\t.Filename = [{filename}]");
            //Log.WriteLine($"\t.Query = [{uri.Query}]");

            if (!uri.LocalPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return false;
            
            if (!uriString.StartsWith(VirtualHost.HostUrl))
            {
                Log.WriteLine($"uri: [{uriString}] !>>> [{VirtualHost.HostUrl}]");
                return false;
            }
                    
            var newRequest = $"{MarkdownConverterPagePath}?dir={directory}&filename={filename}&query={uri.Query}";
            Log.WriteLine($"newRequest [{newRequest}]");
            Log.WriteLine(" ");
            Log.WriteLine(" ");

            WebView2Extensions.WinUiMainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await webView.NavigateToProjectContentFileAsync(newRequest);
                }
                catch (Exception ex)
                {
                    await DialogExtensions.ShowExceptionDialogAsync(webView.XamlRoot!, $"NavigateToProjectContentFileAsync({newRequest})", ex);
                }
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Log.WriteLine($"MarkdownExtensions.OnNavStart : [{ex}]");
            return false;
        }
        
    }
    
}
