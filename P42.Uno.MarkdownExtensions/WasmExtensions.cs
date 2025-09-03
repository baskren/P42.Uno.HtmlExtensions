#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

public static partial class WasmExtensions
{
#if BROWSERWASM

    private static readonly Dictionary<string, WeakReference<WebView2>> WebView2Cache = new ();

    internal static async Task EnableOnLoadAsync(WebView2 webView)
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
        return (webView, href);;
    }
    
    [System.Runtime.InteropServices.JavaScript.JSImport("globalThis.P42_EnableOnLoad")]
    internal static partial string EnableOnLoad(string id);
        
    [System.Runtime.InteropServices.JavaScript.JSExport()]
    internal static async Task<string> OnLoad(string id, string name)
    {
        var msg = $"WasmExtensions.OnLoad: [{id}][{name}]";
        Log.WriteLine(msg);

        var (webview, href) = await GetWebView2AndHrefAsync(id);
        msg += $" : [{href}]";

        if (webview != null)
        {
            Log.WriteLine($"WasmExtensions.OnLoad: [{msg}]");
            MarkdownExtensions.RedirectIfMarkdown(webview, href);
            
        }
        
        return $"id:[{id}] href:[{href}]";
    }        
#endif

}
