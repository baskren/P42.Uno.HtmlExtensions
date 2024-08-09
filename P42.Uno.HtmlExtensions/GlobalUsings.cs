#if __WASM__ 
global using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
global using WebViewNavigationCompletedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationCompletedEventArgs;
global using WebViewNavigationStartingEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationStartingEventArgs;
global using WebViewNavigationFailedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationFailedEventArgs;
global using WebView = P42.Uno.HtmlExtensions.WebViewX;
#elif __IOS__ || ANDROID || MACCATALYST || DESKTOP || WinAppSdk
global using BaseWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
global using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif
