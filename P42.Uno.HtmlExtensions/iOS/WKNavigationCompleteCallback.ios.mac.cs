using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
#if __IOS__
using UIKit;
#else
using AppKit;
#endif
using WebKit;
using Windows.Storage;

namespace P42.Uno.HtmlExtensions
{
    class WKNavigationCompleteCallback : WKNavigationDelegate
    {
        public bool Completed { get; private set; }

        int loadCount;
        readonly string _filename;
        readonly PageSize _pageSize;
        readonly PageMargin _margin;
        readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
        readonly Func<WKWebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> _action;

        public WKNavigationCompleteCallback(string fileName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource, Func<WKWebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> action)
        {
            _filename = fileName;
            _pageSize = pageSize;
            _margin = margin;
            _taskCompletionSource = taskCompletionSource;
            _action = action;
        }

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            loadCount++;
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            loadCount--;
            Timer.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                if (loadCount <= 0)
                {
                    NSRunLoop.Main.BeginInvokeOnMainThread(() => _action?.Invoke(webView, _filename, _pageSize, _margin, _taskCompletionSource));
                    return false;
                }
                return true;
            });

        }
    }

}