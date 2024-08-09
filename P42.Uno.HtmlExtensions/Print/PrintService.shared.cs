using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;


namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Platform Printing
    /// </summary>
    public static class PrintService
    {
#if __IOS__ || __ANDROID__ || WINDOWS || __WASM__
        static INativePrintService _nativePrintService;
        static INativePrintService NativePrintService =>
            _nativePrintService = _nativePrintService ?? new NativePrintService();
#else
        static INativePrintService NativePrintService =>
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
            =>await (NativePrintService?.PrintAsync(uri, jobName) ?? Task.CompletedTask);

        /// <summary>
        /// Print HTML in file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="jobName"></param>
        public static async Task PrintAsync(this StorageFile file, string jobName)
            => await new Uri(file.Path).PrintAsync(jobName);

        /// <summary>
        /// Print HTML string
        /// </summary>
        /// <param name="html"></param>
        /// <param name="jobName"></param>
        public static async Task PrintAsync(this string html, string jobName)
        {
            /*
            var uri = await html.ToTempFileUriAsync();
            await uri.PrintAsync(jobName);
            */
            await (NativePrintService?.PrintAsync(html, jobName) ?? Task.CompletedTask);
        }

        /// <summary>
        /// Print the specified webview and jobName.
        /// </summary>
        /// <param name="webview">Webview.</param>
        /// <param name="jobName">Job name.</param>
        public static async Task PrintAsync(this BaseWebView webview, string jobName)
            => await (NativePrintService?.PrintAsync(webview, jobName) ?? Task.CompletedTask);
        

    }
}
