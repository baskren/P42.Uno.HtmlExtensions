#pragma warning disable CS0067

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using P42.Uno.HtmlExtensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;


namespace P42.UI.Xaml.Controls
{
    public partial class WebView2 : BaseWebView
    {

        #region Properties
        public static DependencyProperty DefaultBackgroundColorProperty = DependencyProperty.RegisterAttached(
                                                                                    nameof(DefaultBackgroundColor),
                                                                                    typeof(Color),
                                                                                    typeof(WebView2),
                                                                                    new PropertyMetadata(default, OnDefaultColorChanged));

        private static void OnDefaultColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is WebView2 view && args.NewValue is Color color)
                view.Background = new SolidColorBrush(color);
        }

        public Color DefaultBackgroundColor
        {
            get => (Color)GetValue(DefaultBackgroundColorProperty);
            set => SetValue(DefaultBackgroundColorProperty, value);
        }
        #endregion


        #region Fields
        TaskCompletionSource<bool> _loadedTcs = new TaskCompletionSource<bool>();
        internal string _id = Guid.NewGuid().ToString();
#if !__WASM__
        internal ulong _instanceId;
        static internal ulong _instances;
#endif

        #endregion


        #region Events
        /// <summary>
        /// Occurs when the core WebView2 process fails.
        /// </summary>
        public event TypedEventHandler<WebView2, P42.Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs> CoreProcessFailed;
        /// <summary>
        /// Occurs when the WebView2 object is initialized.
        /// </summary>
        public event TypedEventHandler<WebView2, P42.UI.Xaml.Controls.CoreWebView2InitializedEventArgs> CoreWebView2Initialized;

#if NET7_0_OR_GREATER && !__WASM__
        /// <summary>
        /// Occurs when the WebView2 has completely loaded (body.onload has been raised) or loading stopped with error.
        /// </summary>
        public new event TypedEventHandler<WebView2, P42.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;
        /// <summary>
        /// Occurs when the main frame of the WebView2 navigates to a different URI.
        /// </summary>
        public new event TypedEventHandler<WebView2, P42.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs> NavigationStarting;
#else
        /// <summary>
        /// Occurs when the WebView2 has completely loaded (body.onload has been raised) or loading stopped with error.
        /// </summary>
        public new event TypedEventHandler<WebView2, P42.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;
        /// <summary>
        /// Occurs when the main frame of the WebView2 navigates to a different URI.
        /// </summary>
        public new event TypedEventHandler<WebView2, P42.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs> NavigationStarting;
#endif

        /// <summary>
        /// Occurs when a new HTML document is loaded.
        /// </summary>
        public event TypedEventHandler<WebView2, P42.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs> WebMessageReceived;

        #endregion


        #region Constructor
        public WebView2()
        {
            _instanceId = _instances++;
#if !NET7_0_OR_GREATER || __WASM__
            base.NavigationStarting += OnBaseNavigationStarting;
            base.NavigationCompleted += OnNavigationCompleted;
#endif

#if __ANDROID__
            if (this.GetAndroidWebView() is Android.Webkit.WebView droidWebView)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2. : ");
            }

            Loaded += WebView2_Loaded;
#endif
        }

        private void WebView2_Loaded(object sender, RoutedEventArgs e)
        {
#if __ANDROID__
            if (this.GetAndroidWebView() is Android.Webkit.WebView droidWebView)
            {
                droidWebView.Settings.AllowFileAccess = true;
                droidWebView.Settings.AllowFileAccessFromFileURLs = true;
                droidWebView.Settings.AllowUniversalAccessFromFileURLs = true;

                if (Source != null)
                    Reload();
            }

            Loaded -= WebView2_Loaded;
#endif
        }



        private void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            NavigationCompleted?.Invoke(this, new Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs
            {
                IsSuccess = args.IsSuccess,
                WebErrorStatus = (Microsoft.Web.WebView2.Core.CoreWebView2WebErrorStatus)args.WebErrorStatus
            });
        }

        private void OnBaseNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var newArgs = new Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs
            {
                Cancel = args.Cancel,
                Uri = args.Uri.AbsoluteUri
            };
            NavigationStarting?.Invoke(this, newArgs);
            args.Cancel = newArgs.Cancel;
        }

#endregion


        #region Public Methods

        public async Task EnsureCoreWebView2Async()
            => await _loadedTcs.Task;

        public async Task<string> ExecuteScriptAsync(string script)
        {
#if (__WASM__ || !NET7_0_OR_GREATER ) && !WINDOWS
            return await InvokeScriptAsync(script, Array.Empty<string>());
#else
            return await Task.FromResult(string.Empty);
#endif
        }

        public void Reload()
        {
#if !NET7_0_OR_GREATER || __WASM__
            Refresh();
#endif
        }

        #endregion



    }



#if __ANDROID__
    class AndroidWebViewClient : Android.Webkit.WebViewClient
    {

        static AndroidWebViewClient _instance;
        public static AndroidWebViewClient Instance => _instance = _instance ?? new AndroidWebViewClient();

        private AndroidWebViewClient() 
        {
            System.Diagnostics.Debug.WriteLine($"AndroidWebViewClient. : ");
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, string url)
        {
            if (new Uri(url) is Uri uri && uri.IsFile && uri.AbsolutePath.StartsWith(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path))
            {
                    view.LoadUrl(url);
                    return true;
            }

            if (Android.OS.Build.VERSION.SdkInt < (Android.OS.BuildVersionCodes)24)
#pragma warning disable CA1422 // Validate platform compatibility
                return base.ShouldOverrideUrlLoading(view, url);
#pragma warning restore CA1422 // Validate platform compatibility

            return false;
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, Android.Webkit.IWebResourceRequest request)
        {
            if (request.Url.Scheme.StartsWith("file") && request.Url.Path.StartsWith(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path))
            {
                view.LoadUrl(request.Url.ToString());
                return true;
            }
            return base.ShouldOverrideUrlLoading(view, request);
        }
    }
#endif
}
