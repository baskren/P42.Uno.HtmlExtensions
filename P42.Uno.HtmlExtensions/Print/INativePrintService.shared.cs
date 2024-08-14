using System;
using System.Threading.Tasks;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Print service.
    /// </summary>
    internal interface INativePrintService
    {
        bool IsAvailable { get; }

        Task PrintAsync(WebView2 webView, string jobName);

        Task PrintAsync(Uri uri, string jobName);

        Task PrintAsync(string html, string jobName);

        void AllowFileAccess(WebView2 webView, bool allow);
        
        void AllowNetworkLoads(WebView2 webView, bool allow);
    }
}
