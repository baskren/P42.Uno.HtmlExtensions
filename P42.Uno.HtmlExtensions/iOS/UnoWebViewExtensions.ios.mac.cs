using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if __IOS__
using UIKit;
#else
using AppKit;
#endif
using WebKit;
using Microsoft.UI.Xaml.Media;

#if __P42WASM__
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#elif !HAS_UNO || NET7_0
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif


namespace P42.Uno.HtmlExtensions
{
    static class UnoWebViewExtensions
    {
        public static Microsoft.UI.Xaml.Controls.UnoWKWebView GetNativeWebView(this BaseWebView unoWebView)
        {
            if (unoWebView.FindSubviewsOfType<WKWebView>().FirstOrDefault() is Microsoft.UI.Xaml.Controls.UnoWKWebView nativeWebView)
                return nativeWebView;
            return null;
        }
    }
}
