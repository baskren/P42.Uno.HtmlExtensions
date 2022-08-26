using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Html string extensions.
    /// </summary>
    public static class ToPngService
    {
#if __IOS__ || __ANDROID__ || NET6_0_WINDOWS10_0_19041_0 
        static INativeToPngService _nativeToPngService;
        static INativeToPngService NativeToPngService =>
            _nativeToPngService = _nativeToPngService ?? new NativeToPngService();
#else   
        static INativeToPngService NativeToPngService =>
            null;
#endif

        /// <summary>
        /// Tests if ToPng service is available.
        /// </summary>
        public static bool IsAvailable => NativeToPngService?.IsAvailable ?? false;

        /// <summary>
        /// Converts HTML text to PNG
        /// </summary>
        /// <param name="html">HTML string to be converted to PNG</param>
        /// <param name="fileName">Name (not path), excluding suffix, of PNG file</param>
        /// <param name="width">Width of resulting PNG (in pixels).</param>
        /// <returns></returns>
        public static async Task<ToFileResult> ToPngAsync(this string html, string fileName, int width = -1)
        {
            if (width <= 0)
                width = (int)Math.Ceiling((PageSize.Default.Width - 0.5) * 4);
            return await (NativeToPngService?.ToPngAsync(html, fileName, width) ?? Task.FromResult(new ToFileResult("PNG Service is not implemented on this platform.")));
        }

        /// <summary>
        /// Creates a PNG from the contents of an Uno WebView
        /// </summary>
        /// <param name="webView">Uno WebView</param>
        /// <param name="fileName">Name (not path), excluding suffix, of PNG file</param>
        /// <param name="width">Width of resulting PNG (in pixels).</param>
        /// <returns></returns>
        public static async Task<ToFileResult> ToPngAsync(this WebView2 webView, string fileName, int width = -1)
        {
            if (width <= 0)
                width = (int)Math.Ceiling((PageSize.Default.Width - (12 * 25.4)) * 4);
            return await (NativeToPngService?.ToPngAsync(webView, fileName, width) ?? Task.FromResult(new ToFileResult("PNG Service is not implemented on this platform.")));
        }
    }
}
