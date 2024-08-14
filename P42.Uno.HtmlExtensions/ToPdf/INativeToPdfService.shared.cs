using System;
using System.Threading.Tasks;


namespace P42.Uno.HtmlExtensions
{
    internal interface INativeToPdfService
    {
        /// <summary>
        /// Is PDF generation available?
        /// </summary>
        bool IsAvailable { get; }


        Task<ToFileResult> ToPdfAsync(Uri uri, string fileName, PageSize pageSize, PageMargin margin);

        Task<ToFileResult> ToPdfAsync(WebView2 webView, string fileName, PageSize pageSize, PageMargin margin);
    }

}
