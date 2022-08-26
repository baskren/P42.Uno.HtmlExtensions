#if __MACOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace P42.Uno.HtmlExtensions
{
    class NativePrintService : INativePrintService
    {
        public bool IsAvailable() => false;

        public Task PrintAsync(WebView2 webView, string jobName)
        {
            throw new NotImplementedException();
        }

        public Task PrintAsync(string html, string jobName)
        {
            throw new NotImplementedException();
        }
    }
}
#endif