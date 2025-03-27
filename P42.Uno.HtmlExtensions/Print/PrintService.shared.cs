using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;


namespace P42.Uno.HtmlExtensions;

/// <summary>
/// Platform Printing
/// </summary>
public static class PrintService
{
#if __IOS__ || __ANDROID__ || WINDOWS || __WASM__
    private static INativePrintService _nativePrintService;
    internal static INativePrintService NativePrintService =>
        _nativePrintService ??= new NativePrintService();
#else
        internal static INativePrintService NativePrintService =>
            null;
#endif

    /// <summary>
    /// Gets a value indicating whether this <see cref="T:Forms9Patch.WebViewExtensions"/> can print.
    /// </summary>
    /// <value><c>true</c> if can print; otherwise, <c>false</c>.</value>
    public static bool IsAvailable => NativePrintService?.IsAvailable ?? false;

    /// <summary>
    /// Print contents of Uri
    /// </summary>
    /// <param name="uri">Uri source</param>
    /// <param name="jobName">print job name</param>
    /// <returns></returns>
    public static async Task PrintAsync(this Uri uri, string jobName)
    {
        ArgumentNullException.ThrowIfNull(uri);
            
        if (uri.Scheme == "ms-appx")
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            await PrintAsync(file, jobName);
            return;
        }


        await (NativePrintService?.PrintAsync(uri, jobName) ?? Task.CompletedTask);
    }

    /// <summary>
    /// Print HTML in file
    /// </summary>
    /// <param name="file"></param>
    /// <param name="jobName"></param>
    public static async Task PrintAsync(this StorageFile file, string jobName)
    {
        ArgumentNullException.ThrowIfNull(file);
        await new Uri(file.Path).PrintAsync(jobName);  
    } 

    /// <summary>
    /// Print HTML string
    /// </summary>
    /// <param name="html"></param>
    /// <param name="jobName"></param>
    public static async Task PrintAsync(this string html, string jobName)
    {
        html ??= string.Empty;
        await (NativePrintService?.PrintAsync(html, jobName) ?? Task.CompletedTask);
    }

    /// <summary>
    /// Print the specified webview and jobName.
    /// </summary>
    /// <param name="webview">Webview.</param>
    /// <param name="jobName">Job name.</param>
    public static async Task PrintAsync(this WebView2 webview, string jobName)
    {
        ArgumentNullException.ThrowIfNull(webview);
        await webview.EnsureCoreWebView2Async();
        //var html = await webview.GetHtmlAsync();
        await (NativePrintService?.PrintAsync(webview, jobName) ?? Task.CompletedTask);  
    } 
        
}
