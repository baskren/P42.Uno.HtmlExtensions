#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

internal static partial class WasmExtensions
{
#if BROWSERWASM

    private static readonly Dictionary<string, WeakReference<WebView2>> WebView2Cache = new ();

    private static readonly Dictionary<string, string> LastPage = new();

    public static async Task EnableOnLoadAsync(WebView2 webView)
    {
        var id = await webView.CoreWebView2.ExecuteScriptAsync("window.frameElement.id");
        id = id?.Trim('"');
        //Log.WriteLine($"WasmExtensions.EnableOnLoadAsync: id: [{id}] ENTER");
        if (!string.IsNullOrEmpty(id))
        {
            EnableOnLoad(id);
            WebView2Cache[id] = new WeakReference<WebView2>(webView);
        }
        //Log.WriteLine($"WasmExtensions.EnableOnLoadAsync: id: [{id}] EXOT");
    }

    private static async Task<(WebView2?, string)> GetWebView2AndHrefAsync(string id)
    {
        if (!WebView2Cache.TryGetValue(id, out var weakWebView))
        {
            var msg = $"WasmExtensions.GetCurrentUrlAsync: id: [{id}] NOT FOUND";
            //Log.WriteLine(msg);
            return (null,msg);
        }
        
        if (!weakWebView.TryGetTarget(out var webView))
        {
            var msg = $"WasmExtensions.GetCurrentUrlAsync: id: [{id}] TARGET NOT FOUND";
            //Log.WriteLine(msg);
            return (null,msg);
        }
        
        var href = await webView.CoreWebView2.ExecuteScriptAsync("window.location.href");
        href = href?.Trim('"');
        Log.WriteLine($"WasmExtensions.GetCurrentUrlAsync: href: [{href}]");
        return (webView, href ?? "");;
    }
    
    [System.Runtime.InteropServices.JavaScript.JSImport("globalThis.P42_EnableOnLoad")]
    public static partial string EnableOnLoad(string id);
        
    [System.Runtime.InteropServices.JavaScript.JSExport()]
    public static async Task<string> OnLoad(string id, string name)
    {
        var msg = $"WasmExtensions.OnLoad: [{id}][{name}]";
        Log.WriteLine(msg);

        var (webview, href) = await GetWebView2AndHrefAsync(id);
        msg += $" : [{href}]";

        if (webview is null || string.IsNullOrEmpty(href))
            return $"WasmExtensions.OnLoad A id:[{id}] href:[{href}]";
        
        if (!href.StartsWith(VirtualHost.HostUrl))
        {
            LastPage[id] = href;
            return $"WasmExtensions.OnLoad B id:[{id}] href:[{href}]";
        }

        var uri = new Uri(href);
        var localPath = uri.LocalPath;
        var directory = Path.GetDirectoryName(localPath);
        var fileName = Path.GetFileName(localPath);
        var extension = Path.GetExtension(localPath);
        var query = uri.Query;
        if (extension != ".md")
        {
            LastPage[id] = href;
            return $"WasmExtensions.OnLoad C id:[{id}] href:[{href}]";
        }

        if (!LastPage.TryGetValue(id, out var lastPage)
            || !lastPage.Contains(MarkdownExtensions.MarkdownConverterPagePath)
            )
        {
            LastPage[id] = href;
            MarkdownExtensions.RedirectIfMarkdown(webview, href);
            return $"WasmExtensions.OnLoad D id:[{id}] href:[{href}]";
        }
        
        var lastPageUrl = new Uri(lastPage);
        var lastPageQuery = lastPageUrl.Query;
        if (string.IsNullOrWhiteSpace(lastPageQuery))
        {
            LastPage[id] = href;
            return $"WasmExtensions.OnLoad E id:[{id}] href:[{href}]";
        }
        
        var queryParams = System.Web.HttpUtility.ParseQueryString(lastPageQuery);
        
        if (queryParams.Get("dir") != directory
            || queryParams.Get("filename") != fileName
            || queryParams.Get("query") != query
            )
        {
            LastPage[id] = href;
            MarkdownExtensions.RedirectIfMarkdown(webview, href);
            return $"WasmExtensions.OnLoad F id:[{id}] href:[{href}]";
        }

        await webview.CoreWebView2.ExecuteScriptAsync("history.back()");
        return $"WasmExtensions.OnLoad G id:[{id}] href:[{href}]";
    }        
#endif

}
