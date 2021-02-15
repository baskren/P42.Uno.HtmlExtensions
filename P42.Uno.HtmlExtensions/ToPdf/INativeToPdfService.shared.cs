using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlExtensions
{
    public interface INativeToPdfService
    {
        bool IsAvailable { get; }

        Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin);

        Task<ToFileResult> ToPdfAsync(WebView webView, string fileName, PageSize pageSize, PageMargin margin);
    }

}
