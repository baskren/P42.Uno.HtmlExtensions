using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.UI.Xaml.Controls;
#if __WASM__
using WebView2 = P42.Uno.HtmlExtensions.WebViewX;
#else
using WebView2 = Microsoft.UI.Xaml.Controls.WebView2;
#endif

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Platform Printing
    /// </summary>
    public static class PrintService
    {
#if __IOS__ || __ANDROID__ || NET6_0_WINDOWS10_0_19041_0 || __WASM__
        static INativePrintService _nativePrintService;
        static INativePrintService NativePrintService =>
            _nativePrintService = _nativePrintService ?? new NativePrintService();
#else
        static INativePrintService NativePrintService =>
            null;
#endif

        /// <summary>
        /// Print the specified webview and jobName.
        /// </summary>
        /// <param name="webview">Webview.</param>
        /// <param name="jobName">Job name.</param>
        public static async Task PrintAsync(this WebView2 webview, string jobName)
        {
            await (NativePrintService?.PrintAsync(webview, jobName) ?? Task.CompletedTask);
        }

        /// <summary>
        /// Print HTML string
        /// </summary>
        /// <param name="html"></param>
        /// <param name="jobName"></param>
        public static async Task PrintAsync(this string html, string jobName)
        {
            await (NativePrintService?.PrintAsync(html, jobName) ?? Task.CompletedTask);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Forms9Patch.WebViewExtensions"/> can print.
        /// </summary>
        /// <value><c>true</c> if can print; otherwise, <c>false</c>.</value>
        public static bool IsAvailable => NativePrintService?.IsAvailable() ?? false;
    }
}
