using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Uno.Foundation;
using Uno.UI;
using Microsoft.UI.Xaml;

namespace P42.Uno.HtmlExtensions
{
    class NativeToPngService : INativeToPngService
    {
        public bool IsAvailable => true;

        public Task<ToFileResult> ToPngAsync(string html, string fileName, int width)
        {
            throw new NotImplementedException();
        }

        public async Task<ToFileResult> ToPngAsync(WebView2 webView, string fileName, int width)
        {
            var id = webView.GetHtmlAttribute("id");
            var result = await WebAssemblyRuntime.InvokeAsync($"UnoScreenshot_GetUrlPromise('{id}', {width})");
            Console.WriteLine("PlatformCaptureAsync result:" + result);
            if (result.StartsWith("success: true"))
            {

            }
            return null;
        }
    }
}
