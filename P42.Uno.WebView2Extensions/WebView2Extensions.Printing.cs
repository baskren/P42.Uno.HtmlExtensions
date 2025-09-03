using System.Reflection;

namespace P42.Uno;

public static partial class WebView2Extensions
{
    /// <summary>
    /// Is printing available on this platform
    /// </summary>
    /// <returns></returns>
    public static bool CanPrint =>
        #if __DESKTOP__
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        #else
        true;
        #endif

    /// <summary>
    /// Print the contents of a WebView2
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="token"></param>
    public static async Task PrintAsync(this WebView2 webView2, CancellationToken token = default)
    {
        await webView2.WaitForDocumentLoadedAsync(token);
            
        #if __ANDROID__
        var nativeWebViewWrapper = webView2.GetNativeWebViewWrapper();
        var type = nativeWebViewWrapper.GetType();
        if (type.GetProperty
        (
            "WebView", 
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        )
        ?.GetValue(nativeWebViewWrapper) is not Android.Webkit.WebView droidWebView)
        throw new Exception("Unable to obtain native webview");

        await droidWebView.PrintAsync(cancellationToken: token);
        #elif __IOS__
        var nativeWebViewWrapper = webView2.GetNativeWebViewWrapper();
        if (nativeWebViewWrapper is not WebKit.WKWebView wkWebView)
            throw new Exception("Unable to obtain native webview");

        var result = await wkWebView.PrintAsync();
        #else
        if (!CanPrint)
            throw new NotSupportedException("PrintAsync is not supported on this platform");
        await webView2.ExecuteScriptAsync("print();").AsTask(token);
        #endif
        
    }

    /// <summary>
    /// Try printing WebView content : presents errors in a dialog
    /// </summary>
    /// <param name="webView2">unknown result if not on the current page</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<bool> TryPrintAsync(this WebView2 webView2, CancellationToken token = default)
    {
        try
        {
            await PrintAsync(webView2, token);
            return true;
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(webView2.XamlRoot!, "WebView2 Print", ex);
            return false;
        }
        
    }

}
