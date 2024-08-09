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

        Task PrintAsync(BaseWebView webView, string jobName);

        Task PrintAsync(Uri uri, string jobName);

        Task PrintAsync(string html, string jobName);
    }
}
