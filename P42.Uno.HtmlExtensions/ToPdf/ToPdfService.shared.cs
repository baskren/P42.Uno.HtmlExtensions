using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Html string extensions.
    /// </summary>
    public static class ToPdfService
    {
#if __IOS__ || __ANDROID__ 
        static INativeToPdfService _nativeToPdfService;
        static INativeToPdfService NativeToPdfService =>
            _nativeToPdfService = _nativeToPdfService ?? new NativeToPdfService();
#else
        static INativeToPdfService NativeToPdfService =>
            null;
#endif

        /// <summary>
        /// Returns true if PDF generation is available on this device
        /// </summary>
        public static bool IsAvailable => NativeToPdfService?.IsAvailable ?? false;

        /// <summary>
        /// Converts HTML text to PNG
        /// </summary>
        /// <param name="html">HTML string to be converted to PDF</param>
        /// <param name="fileName">Name (not path), excluding suffix, of PDF file</param>
        /// <param name="pageSize">PDF page size, in points. (default based upon user's region)</param>
        /// <param name="margin">PDF page's margin, in points. (default is zero)</param>
        /// <returns></returns>
        public static async Task<ToFileResult> ToPdfAsync(this string html, string fileName, PageSize pageSize = default, PageMargin margin = default)
        {
            if (pageSize is null || pageSize.Width <= 0 || pageSize.Height <= 0)
                pageSize = PageSize.Default;

            margin = margin ?? new PageMargin();
            if (pageSize.Width - margin.HorizontalThickness < 1 || pageSize.Height - margin.VerticalThickness < 1)
                return new ToFileResult("Page printable area (page size - margins) has zero width or height.");

            return await (NativeToPdfService?.ToPdfAsync(html, fileName, pageSize, margin) ?? Task.FromResult(new ToFileResult("PDF Service is not implemented on this platform.")));
        }

        /// <summary>
        /// Creates a PNG from the contents of an Uno WebView
        /// </summary>
        /// <param name="webView">Uno WebView</param>
        /// <param name="fileName">Name (not path), excluding suffix, of PDF file</param>
        /// <param name="pageSize">PDF page size, in points. (default based upon user's region)</param>
        /// <param name="margin">PDF page's margin, in points. (default is zero)</param>
        /// <returns>Forms9Patch.ToFileResult</returns>
        public static async Task<ToFileResult> ToPdfAsync(this WebView2 webView, string fileName, PageSize pageSize = default, PageMargin margin = default)
        {
            if (pageSize is null || pageSize.Width <= 0 || pageSize.Height <= 0)
                pageSize = PageSize.Default;

            margin = margin ?? new PageMargin();
            if (pageSize.Width - margin.HorizontalThickness < 1 || pageSize.Height - margin.VerticalThickness < 1)
                return new ToFileResult("Page printable area (page size - margins) has zero width or height.");

            return  await (NativeToPdfService?.ToPdfAsync(webView, fileName, pageSize, margin) ?? Task.FromResult(new ToFileResult("PDF Service is not implemented on this platform.")));
        }
    }
}
