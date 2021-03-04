using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using WebKit;
using Windows.UI.Xaml.Media;

namespace P42.Uno.HtmlExtensions
{
    static class UnoWebViewExtensions
    {
        public static Windows.UI.Xaml.Controls.UnoWKWebView GetNativeWebView(this Windows.UI.Xaml.Controls.WebView unoWebView)
        {
            if (unoWebView.FindSubviewsOfType<WKWebView>().FirstOrDefault() is Windows.UI.Xaml.Controls.UnoWKWebView nativeWebView)
                return nativeWebView;
            return null;
        }
    }
}
