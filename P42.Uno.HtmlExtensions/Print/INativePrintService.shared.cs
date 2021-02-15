using System;
using System.Threading.Tasks;
#if __WASM__
using WebView = P42.Uno.HtmlExtensions.WebViewX;
#else
using WebView = Windows.UI.Xaml.Controls.WebView;
#endif

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Print service.
    /// </summary>
    public interface INativePrintService
    {
        /// <summary>
        /// Print the specified webView and jobName.
        /// </summary>
        /// <param name="webView">Web view.</param>
        /// <param name="jobName">Job name.</param>
        Task PrintAsync(WebView webView, string jobName);

        /// <summary>
        /// Print the specified HTML with jobName
        /// </summary>
        /// <param name="html"></param>
        /// <param name="jobName"></param>
        Task PrintAsync(string html, string jobName);

        /// <summary>
        /// Cans the print.
        /// </summary>
        /// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
        bool IsAvailable();

    }
}
