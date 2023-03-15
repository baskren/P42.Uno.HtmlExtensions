using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Uno;
using Windows.Foundation;
using Windows.Storage.Streams;



namespace P42.Web.WebView2.Core
{
    public sealed class CoreWebView2 : IEquatable<CoreWebView2>
    {
        static internal ulong _instances;
        ulong _instance = _instances++;

        #region Properties
        public ulong BrowserProcessId => _parentWebView?._instanceId ?? 0;

#if __P42WASM__ || !NET7_0
        public bool CanGoBack => _parentWebView?.CanGoBack ?? false;
#else
        public bool CanGoBack => false;
#endif

#if __P42WASM__ || !NET7_0
        public bool CanGoForward => _parentWebView?.CanGoForward ?? false;
#else
        public bool CanGoForward => false;
#endif

        public bool ContainsFullScreenElement
        {
            get
            {
#if __P42WASM__ || !NET7_0 
                var task = Task.Run(async () =>
                {
                    if (_parentWebView is P42.UI.Xaml.Controls.WebView2 w2)
                    {
                        var result = await w2.InvokeScriptAsync(CancellationToken.None, "Document.fullscreenElement", new string[] { });
                        return result;
                    }
                    return string.Empty;
                });
                return !string.IsNullOrWhiteSpace(task.Result);
#else
                return false;
#endif
            }
        }


        public CoreWebView2CookieManager CookieManager { get; private set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2DefaultDownloadDialogCornerAlignment DefaultDownloadDialogCornerAlignment { get; set; }


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public Point DefaultDownloadDialogMargin { get; set; }

#if __P42WASM__ || !NET7_0 
        public string DocumentTitle => _parentWebView?.DocumentTitle ?? string.Empty;
#else
        public string DocumentTitle => string.Empty;
#endif

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2Environment Environment { get; private set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool IsDefaultDownloadDialogOpen { get; private set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool IsDocumentPlayingAudio { get; private set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool IsMuted { get; set; }

        public bool IsSuspended { get; private set; }

        public CoreWebView2Settings Settings { get; private set; }

#if __P42WASM__ || !NET7_0 
        public string Source => _parentWebView.Source.AbsoluteUri; // { get; private set; }
#else
        public string Source => string.Empty;
#endif

        public string StatusBarText { get; private set; }

        public event TypedEventHandler<CoreWebView2, CoreWebView2BasicAuthenticationRequestedEventArgs> BasicAuthenticationRequested;

        public event TypedEventHandler<CoreWebView2, CoreWebView2ContextMenuRequestedEventArgs> ContextMenuRequested;

        public event TypedEventHandler<CoreWebView2, object> StatusBarTextChanged;

        public event TypedEventHandler<CoreWebView2, CoreWebView2DOMContentLoadedEventArgs> DOMContentLoaded;

        public event TypedEventHandler<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs> WebResourceResponseReceived;

        public event TypedEventHandler<CoreWebView2, CoreWebView2DownloadStartingEventArgs> DownloadStarting;

        public event TypedEventHandler<CoreWebView2, CoreWebView2FrameCreatedEventArgs> FrameCreated;

        public event TypedEventHandler<CoreWebView2, CoreWebView2ClientCertificateRequestedEventArgs> ClientCertificateRequested;

        public event TypedEventHandler<CoreWebView2, object> IsDocumentPlayingAudioChanged;

        public event TypedEventHandler<CoreWebView2, object> IsMutedChanged;

        public event TypedEventHandler<CoreWebView2, object> IsDefaultDownloadDialogOpenChanged;

        public event TypedEventHandler<CoreWebView2, object> ContainsFullScreenElementChanged;

        public event TypedEventHandler<CoreWebView2, CoreWebView2ContentLoadingEventArgs> ContentLoading;

        public event TypedEventHandler<CoreWebView2, object> DocumentTitleChanged;

        public event TypedEventHandler<CoreWebView2, CoreWebView2NavigationCompletedEventArgs> FrameNavigationCompleted;

        public event TypedEventHandler<CoreWebView2, CoreWebView2NavigationStartingEventArgs> FrameNavigationStarting;

        public event TypedEventHandler<CoreWebView2, object> HistoryChanged;

        public event TypedEventHandler<CoreWebView2, CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;

        public event TypedEventHandler<CoreWebView2, CoreWebView2NavigationStartingEventArgs> NavigationStarting;

        public event TypedEventHandler<CoreWebView2, CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

        public event TypedEventHandler<CoreWebView2, CoreWebView2PermissionRequestedEventArgs> PermissionRequested;

        public event TypedEventHandler<CoreWebView2, CoreWebView2ProcessFailedEventArgs> ProcessFailed;

        public event TypedEventHandler<CoreWebView2, CoreWebView2ScriptDialogOpeningEventArgs> ScriptDialogOpening;

        public event TypedEventHandler<CoreWebView2, CoreWebView2SourceChangedEventArgs> SourceChanged;

        public event TypedEventHandler<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs> WebMessageReceived;

        public event TypedEventHandler<CoreWebView2, CoreWebView2WebResourceRequestedEventArgs> WebResourceRequested;

        public event TypedEventHandler<CoreWebView2, object> WindowCloseRequested;
        #endregion


        #region  Fields
        P42.UI.Xaml.Controls.WebView2 _parentWebView
        {
            get
            {
                if (_weakWebView?.TryGetTarget(out var value) ?? false)
                    return value;
                return null;
            }
            set
            {
                if (_weakWebView != null)
                    _weakWebView.SetTarget(value);
                else
                    _weakWebView =  new WeakReference<UI.Xaml.Controls.WebView2>(value);
            }
        }
        WeakReference<P42.UI.Xaml.Controls.WebView2> _weakWebView;
        #endregion


        #region  Construction
        internal CoreWebView2(P42.UI.Xaml.Controls.WebView2 webView2)
        {
            _parentWebView = webView2;
            CookieManager = new CoreWebView2CookieManager(_parentWebView);
            Environment = new CoreWebView2Environment();
            Settings = new CoreWebView2Settings();
#if __P42WASM__
            ((P42.Uno.HtmlExtensions.WebViewX)webView2).NavigationStarting += OnXNavStarting;
            ((P42.Uno.HtmlExtensions.WebViewX)webView2).NavigationCompleted += OnXNavCompleted;

#elif !NET7_0
            ((Microsoft.UI.Xaml.Controls.WebView)webView2).NavigationStarting += OnNavStarting;
            ((Microsoft.UI.Xaml.Controls.WebView)webView2).NavigationCompleted += OnNavCompleted;
#endif
        }


#if __P42WASM__ || !NET7_0


#if __P42WASM__
        private void OnXNavCompleted(Uno.HtmlExtensions.WebViewX sender, Uno.HtmlExtensions.WebViewXNavigationCompletedEventArgs args)
#else
        private void OnNavCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
#endif
        {
            var newArgs = new CoreWebView2NavigationCompletedEventArgs
            {
                IsSuccess = args.IsSuccess,
                WebErrorStatus = (Microsoft.Web.WebView2.Core.CoreWebView2WebErrorStatus)args.WebErrorStatus,
            };
            NavigationCompleted?.Invoke(this, newArgs);
        }

#if __P42WASM__
        private void OnXNavStarting(Uno.HtmlExtensions.WebViewX sender, Uno.HtmlExtensions.WebViewXNavigationStartingEventArgs args)
#else
        private void OnNavStarting(WebView sender, WebViewNavigationStartingEventArgs args)
#endif
        {
            var newArgs = new CoreWebView2NavigationStartingEventArgs
            {
                Uri = args.Uri.AbsoluteUri,
                Cancel = args.Cancel,
            };
            NavigationStarting?.Invoke(this, newArgs);
            args.Cancel = newArgs.Cancel;
        }

#endif

#endregion


        #region  Operators
        public static bool operator ==(CoreWebView2 x, CoreWebView2 y)
        {
            return x?._instance == y?._instance;
        }

        public static bool operator !=(CoreWebView2 x, CoreWebView2 y)
        {
            return !(x == y);
        }

        public bool Equals(CoreWebView2 other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            CoreWebView2 coreWebView = obj as CoreWebView2;
            if ((object)coreWebView != null)
            {
                return this == coreWebView;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<string> CallDevToolsProtocolMethodForSessionAsync(string sessionId, string methodName, string parametersAsJson)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void NavigateWithWebResourceRequest(CoreWebView2WebResourceRequest Request)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<bool> TrySuspendAsync()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void Resume()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void SetVirtualHostNameToFolderMapping(string hostName, string folderPath, CoreWebView2HostResourceAccessKind accessKind)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void ClearVirtualHostNameToFolderMapping(string hostName)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void OpenTaskManagerWindow()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<bool> PrintToPdfAsync(string ResultFilePath, CoreWebView2PrintSettings printSettings)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void OpenDefaultDownloadDialog()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void CloseDefaultDownloadDialog()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void Navigate(string uri)
        {
#if __P42WASM__ || !NET7_0
            Console.WriteLine($"CoreWebView2[{this._parentWebView._id}].Navigate ENTER {uri} ");
            if (_parentWebView is P42.UI.Xaml.Controls.WebView2 w2)
                w2.Navigate(new Uri(uri));
            Console.WriteLine($"CoreWebView2[{this._parentWebView._id}].Navigate ENTER {uri}");
#endif
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void NavigateToString(string htmlContent)
        {
#if __P42WASM__ || !NET7_0
            Console.WriteLine($"CoreWebView2[{this._parentWebView._id}].NavigateToString ENTER ");
            if (_parentWebView is P42.UI.Xaml.Controls.WebView2 w2)
            {
                var uri = Task.Run(async () => await P42.Uno.HtmlExtensions.StringExtensions.ToTempFileUriAsync(htmlContent)).Result;
                w2.Navigate(uri);
            }
            Console.WriteLine($"CoreWebView2[{this._parentWebView._id}].NavigateToString EXIT ");
#endif
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<string> AddScriptToExecuteOnDocumentCreatedAsync(string javaScript)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void RemoveScriptToExecuteOnDocumentCreated(string id)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<string> ExecuteScriptAsync(string javaScript)
        {
            if (_parentWebView is P42.UI.Xaml.Controls.WebView2 w2)
                return w2.ExecuteScriptAsync(javaScript).AsAsyncOperation<string>();
            return Task.FromResult(string.Empty).AsAsyncOperation<string>();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncAction CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat imageFormat, IRandomAccessStream imageStream)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void Reload()
        {
            _parentWebView?.Reload();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void PostWebMessageAsJson(string webMessageAsJson)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void PostWebMessageAsString(string webMessageAsString)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<string> CallDevToolsProtocolMethodAsync(string methodName, string parametersAsJson)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void GoBack()
        {
#if __P42WASM__ || !NET7_0
            _parentWebView?.GoBack();
#endif
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void GoForward()
        {
#if __P42WASM__ || !NET7_0
            _parentWebView?.GoForward();
#endif
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2DevToolsProtocolEventReceiver GetDevToolsProtocolEventReceiver(string eventName)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void Stop()
        {
#if __P42WASM__ || !NET7_0
            _parentWebView?.Stop();
#endif
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void AddHostObjectToScript(string name, object rawObject)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void RemoveHostObjectFromScript(string name)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void OpenDevToolsWindow()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void AddWebResourceRequestedFilter(string uri, CoreWebView2WebResourceContext ResourceContext)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void RemoveWebResourceRequestedFilter(string uri, CoreWebView2WebResourceContext ResourceContext)
        {
            throw new NotImplementedException();
        }

    }
}
