using System;
using System.Web;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using P42.Uno;
using Log = System.Console;
//using Log = System.Diagnostics.Debug;
namespace P42.Uno;

public static class WebViewMarkdownExtensions
{
    public static void EnableMarkdownSupport(this WebView2 webView)
    {
        //TODO: Add VirtualHost.LocalFolders.AddDistinct("UnoLib1"); here
        //webView.IsMarkdownVirtualHostMapped(true);
        webView.EnableProjectContentFolder("P42.Uno.MarkdownExtensions");
        webView.NavigationStarting += OnNavStart;
    }

    public static readonly DependencyProperty IsMarkdownVirtualHostMappedProperty =
        DependencyProperty.RegisterAttached(
            "IsMarkdownVirtualHostMappedProperty", // Name of the attached property
            typeof(bool),     // Type of the attached property
            typeof(WebViewMarkdownExtensions), // Owner type (the static class)
            new PropertyMetadata(false)); // Optional: default value and property changed callback

    //public static bool IsMarkdownVirtualHostMapped(this WebView2 webView) => (bool)webView.GetValue(IsMarkdownVirtualHostMappedProperty);
    
    //public static void IsMarkdownVirtualHostMapped(this WebView2 webView, bool value)  => webView.SetValue(IsMarkdownVirtualHostMappedProperty, value);
    
    private static void OnNavStart(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {

        //var json = System.Text.Json.JsonSerializer.Serialize(args);
        //Log.WriteLine($"args.AdditionalAllowedFrameAncestors [{args.AdditionalAllowedFrameAncestors}]");
        Log.WriteLine($"args.Cancel [{args.Cancel}]");
        Log.WriteLine($"args.IsRedirected [{args.IsRedirected}]");
        Log.WriteLine($"args.IsUserInitiated [{args.IsUserInitiated}]");
        Log.WriteLine($"args.NavigationId [{args.NavigationId}]");
        //Log.WriteLine($"args.NavigationKind [{args.NavigationKind}]");
        //Log.WriteLine($"args.RequestHeaders [{args.RequestHeaders}]");
        Log.WriteLine($"args.Uri [{args.Uri}]");
        
        // Desktop.Mac:
        // args.Cancel [False]
        // args.IsRedirected [False]
        // args.IsUserInitiated [False]
        // args.NavigationId [1]
        // args.Uri [file:///Users/ben/Development/Uno/WebViewUtils/WebViewUtils/bin/Debug/net9.0-desktop/WebContent/CltInstall.html]
        // Cancel works
        
        // iOS:
        // args.Cancel [False]
        // args.IsRedirected [False]
        // args.IsUserInitiated [False]
        // args.NavigationId [1]
        // args.Uri [file:///Users/ben/Library/Developer/CoreSimulator/Devices/B17C00C2-92CB-4C9D-AD02-A64E1791D467/data/Containers/Bundle/Application/4622E99A-5C37-4C65-843D-AC21B0DC8EDA/WebViewUtils.app/WebContent/CltInstall.html]
        // Cancel works
        
        // Android:
        // args.Cancel [False]
        // args.IsRedirected [False]
        // args.IsUserInitiated [False]
        // args.NavigationId [1]
        // args.Uri [file:///android_asset/WebContent/CltInstall.html]
        // Cancel works
        
        // Wasm:
        // args.Cancel [False]
        // dotnet.native.2vn64vi5fk.js:1685 args.IsRedirected [False]
        // dotnet.native.2vn64vi5fk.js:1685 args.IsUserInitiated [False]
        // dotnet.native.2vn64vi5fk.js:1685 args.NavigationId [1]
        // dotnet.native.2vn64vi5fk.js:1685 args.Uri [data:text/html;charset=utf-8;base64,L3BhY2thZ2VfMmE0YzBiYTgxMWQwN2U2OTBmNzkxMjI4ZTE2ZTgyNTgxM2EwMDM5Yi9XZWJDb250ZW50L0NsdEluc3RhbGwuaHRtbA==]
        // above is the same as :/package_2a4c0ba811d07e690f791228e16e825813a0039b/WebContent/CltInstall.html
        // Cancel works

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
        
        if (!uri.LocalPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            return;

        if (uri.Query is string query && !string.IsNullOrWhiteSpace(query))
        {
            var queryParams = HttpUtility.ParseQueryString(query);
            foreach (var key in queryParams.AllKeys)
                Log.WriteLine($"QUERY {key}: {queryParams[key]}");
        }
        
        
        args.Cancel = true;
        
        var requestedSource = sender.Source;
        Log.WriteLine($"requestedSource [{requestedSource}]");
        
        
        
        var newSource = WebView2Extensions.ProjectContentFileUri($"P42.Uno.MarkdownExtensions/MarkdownPage3.html");
        Log.WriteLine($"newSource [{newSource}]");
        var relativePath = Path.GetRelativePath(newSource.AbsolutePath, requestedSource.AbsolutePath);
        Log.WriteLine($"relativePath [{relativePath}]");
        var safeRelativePath = System.Web.HttpUtility.UrlEncode(relativePath);
        Log.WriteLine($"safeRelativePath [{safeRelativePath}]");
        var newRequest = $"P42.Uno.MarkdownExtensions/MarkdownPage3.html?requestUrl={safeRelativePath}";
        Log.WriteLine($"newRequest [{newRequest}]");
        Log.WriteLine(" ");
        Log.WriteLine(" ");
        //var uriFriendly = Uri.EscapeUriString(uri);
        /*
        //var newSource = $"https://UnoLib1/WebContent/MarkdownPage3.html?requestUrl={uriFriendly}";
        Log.WriteLine(newSource);
        sender.Source = new Uri(newSource);
        */
        Task.Run(async () =>
        {
            //await Task.Delay(500);
            sender.NavigateToProjectContentFile(newRequest);
        });
    }
}
