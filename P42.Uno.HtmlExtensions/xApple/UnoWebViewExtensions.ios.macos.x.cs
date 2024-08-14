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



namespace P42.Uno.HtmlExtensions
{
    static class UnoWebViewExtensions
    {
        public static Microsoft.UI.Xaml.Controls.UnoWKWebView GetNativeWebView(this WebView2 unoWebView)
        {
            if (unoWebView.FindSubviewsOfType<WKWebView>().FirstOrDefault() is Microsoft.UI.Xaml.Controls.UnoWKWebView nativeWebView)
                return nativeWebView;
            return null;
        }
    }
}
