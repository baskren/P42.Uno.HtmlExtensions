using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace P42.Uno.HtmlExtensions
{
    public interface INativeToPdfService
    {
        bool IsAvailable { get; }

        Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin);

        Task<ToFileResult> ToPdfAsync(WebView2 webView, string fileName, PageSize pageSize, PageMargin margin);
    }

}
