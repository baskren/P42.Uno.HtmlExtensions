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
    /// <summary>
    /// HTML to PDF service.
    /// </summary>
    public class NativeToPngService : INativeToPngService
    {
        const string LocalStorageFolderName = "P42.Uno.HtmlWevViewExtensions.ToPngService";


#if __IOS__
        public bool IsAvailable => UIPrintInteractionController.PrintingAvailable && NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0));
#else
        public bool IsAvailable => true;
#endif

        public async Task<ToFileResult> ToPngAsync(string html, string fileName, int width)
        {
            if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            {
                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                const string jScript = @"var meta = document.createElement('meta'); meta.setAttribute('name', 'viewport'); meta.setAttribute('content', 'width=device-width'); document.getElementsByTagName('head')[0].appendChild(meta);";
                var wkUScript = new WKUserScript((NSString)jScript, WKUserScriptInjectionTime.AtDocumentEnd, true);
                using (var wkUController = new WKUserContentController())
                {
                    wkUController.AddUserScript(wkUScript);
                    var configuration = new WKWebViewConfiguration
                    {
                        UserContentController = wkUController
                    };
                    using (var webView = new WKWebView(new CGRect(0, 0, width, width), configuration)
                    {
#if __IOS__
                        UserInteractionEnabled = false,
                        BackgroundColor = UIColor.White
#endif
                    })
                    {
                        webView.NavigationDelegate = new WKNavigationCompleteCallback(fileName, new PageSize { Width = width }, null, taskCompletionSource, NavigationCompleteAsync);
                        webView.LoadHtmlString(html, null);
                        return await taskCompletionSource.Task;
                    }
                }
            }
            return await Task.FromResult(new ToFileResult("PNG output not available prior to iOS 11"));
        }

        public async Task<ToFileResult> ToPngAsync(Windows.UI.Xaml.Controls.WebView unoWebView, string fileName, int width)
        {
            if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            {
                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                if (unoWebView.GetNativeWebView() is Windows.UI.Xaml.Controls.UnoWKWebView wkWebView)
                {
#if __IOS__
                    wkWebView.BackgroundColor = UIColor.White;
                    wkWebView.UserInteractionEnabled = false;
#endif
                    wkWebView.NavigationDelegate = new WKNavigationCompleteCallback(fileName, new PageSize { Width = wkWebView.Bounds.Width, Height = wkWebView.Bounds.Height }, null, taskCompletionSource, NavigationCompleteAsync);
                    return await taskCompletionSource.Task;
                }
                return await Task.FromResult(new ToFileResult("Could not get NativeWebView for Uno WebView"));
            }
            return await Task.FromResult(new ToFileResult("PNG output not available prior to iOS 11"));
        }


        static async Task NavigationCompleteAsync(WKWebView webView, string filename, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            try
            {
                var widthString = await webView.EvaluateJavaScriptAsync("document.documentElement.offsetWidth");
                var width = double.Parse(widthString.ToString());

                var heightString = await webView.EvaluateJavaScriptAsync("document.documentElement.offsetHeight");
                var height = double.Parse(heightString.ToString());

                if (width < 1 || height < 1)
                {
                    taskCompletionSource.SetResult(new ToFileResult("WebView has zero width or height"));
                    return;
                }
#if __IOS__
                webView.ClipsToBounds = false;
                webView.ScrollView.ClipsToBounds = false;
#endif
                var bounds = webView.Bounds;
                webView.Bounds = new CGRect(0, 0, (nfloat)width, (nfloat)height);

                var scale = pageSize.Width / width;

#if __IOS__
                var displayScale = UIScreen.MainScreen.Scale;
#else
                var mainScreen = NSScreen.MainScreen;
                var displayScale = mainScreen.BackingScaleFactor;
#endif


                var snapshotConfig = new WKSnapshotConfiguration
                {
                    SnapshotWidth = pageSize.Width / displayScale
                };

                var image = await webView.TakeSnapshotAsync(snapshotConfig);

                if (image.AsPNG() is NSData data)
                {
                    var folder = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());
                    var file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        await stream.WriteAsync(new ReadOnlyMemory<byte>(data.ToArray()));
                    }
                    taskCompletionSource.SetResult(new ToFileResult(file));
                    return;
                }
                webView.Bounds = bounds;
                taskCompletionSource.SetResult(new ToFileResult("No data returned."));
            }
            catch (Exception e)
            {

                taskCompletionSource.SetResult(new ToFileResult("Exception: " + e.Message + (e.InnerException != null
                    ? "Inner exception: " + e.InnerException.Message
                    : null)));
            }
            finally
            {
                webView.Dispose();
            }

        }
    }


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
