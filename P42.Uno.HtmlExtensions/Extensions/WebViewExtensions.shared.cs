using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.WebUI;

#if __P42WASM__
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#elif !HAS_UNO || NET7_0
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView2;
#else
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif



namespace P42.Uno.HtmlExtensions
{
    public static partial class WebViewExtensions
    {

        public static void NavigateToResource(this BaseWebView webView, string resourceId, Assembly assembly)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceId))
            {
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd();
                    var path = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, $"{Guid.NewGuid()}.html");
                    File.WriteAllText(path, text);
#if __P42WASM__ || !NET7_0
                    webView.Source = new Uri($"file://{path}");
#endif
                }
            }
        }

        struct TryResult<T>
        {
            public bool IsSuccess { get; set; }

            public T Value { get; set; }

            public TryResult(bool success, T value = default)
            {
                IsSuccess = success;
                Value = value;
            }
        }
        
        static async Task<TryResult<int>> TryExecuteIntScriptAsync(this BaseWebView webView2, string script)
        {
#if __P42WASM__ || !NET7_0
            try
            {
                var result = await webView2.ExecuteScriptAsync(script);
                if (int.TryParse(result, out int v))
                    return new TryResult<int>(true, v);
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine($"WebViewExtensions.TryExecuteIntScriptAsync {ex.GetType()} : {ex.Message} \n{ex.StackTrace} ");
            }
#endif
            return await Task.FromResult(new TryResult<int>(false));
        }

        static async Task<TryResult<double>> TryExecuteDoubleScriptAsync(this BaseWebView webView2, string script)
        {
#if __P42WASM__ || !NET7_0
            try
            {
                var result = await webView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"WebViewExtensions.TryExecuteDoubleScriptAsync : [{script}] : [{result}]");
                if (double.TryParse(result, out var v))
                    return new TryResult<double>(true, v);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebViewExtensions.TryExecuteDoubleScriptAsync {ex.GetType()} : {ex.Message} \n{ex.StackTrace} ");
            }
#endif
            return await Task.FromResult(new TryResult<double>(false));
        }

        static async Task<double> TryUpdateIfLarger(this BaseWebView webView2, string script, double source)
        {
            if (await webView2.TryExecuteDoubleScriptAsync(script) is TryResult<double> r1 && r1.IsSuccess && r1.Value > source)
                return r1.Value;

            return source;
        }

        /// <summary>
        /// Get the size of a WebView's current content
        /// </summary>
        /// <param name="webView"></param>
        /// <param name="depth"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        public static async Task<Windows.Foundation.Size> WebViewContentSizeAsync(this BaseWebView webView, int depth = 0, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
        {
            if (webView is null)
                throw new ArgumentNullException(nameof(webView));

            //var hzAlign = webView.HorizontalAlignment;
            //webView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;

            //System.Diagnostics.Debug.WriteLine("WebViewExtensions. webView Actual=[" + webView.ActualWidth + ", " + webView.ActualHeight + "]");
            //System.Diagnostics.Debug.WriteLine("WebViewExtensions. webView Desired=[" + webView.DesiredSize + "]");

            //var contentWidth = PageSize.Default.Width;
            //var contentHeight = PageSize.Default.Height;
            double contentWidth = -1;
            double contentHeight = -1;

            if (depth > 50)
                return new Windows.Foundation.Size(contentWidth, contentHeight);
            if (depth > 0)
                await Task.Delay(100);



            try
            {
                contentWidth = await webView.TryUpdateIfLarger("document.documentElement.scrollWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.documentElement.scrollHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.documentElement.offsetWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.documentElement.offsetHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.documentElement.getBoundingClientRect().width", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.documentElement.getBoundingClientRect().height", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.documentElement.clientWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.documentElement.clientHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.documentElement.innerWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.documentElement.innerHeight", contentHeight);



                contentWidth = await webView.TryUpdateIfLarger("self.scrollWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("self.scrollHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("self.offsetWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("self.offsetHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("self.getBoundingClientRect().width", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("self.getBoundingClientRect().height", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("self.clientWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("self.clientHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("self.innerWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("self.innerHeight", contentHeight);



                contentWidth = await webView.TryUpdateIfLarger("document.body.scrollWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.body.scrollHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.body.offsetWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.body.offsetHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.body.getBoundingClientRect().width", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.body.getBoundingClientRect().height", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.body.clientWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.body.clientHeight", contentHeight);

                contentWidth = await webView.TryUpdateIfLarger("document.body.innerWidth", contentWidth);
                contentHeight = await webView.TryUpdateIfLarger("document.body.innerHeight", contentHeight);

                /*
                var docScrollWidth = await webView.ExecuteScriptAsync("document.documentElement.scrollWidth.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.scrollWidth " + docScrollWidth);
                //if (double.TryParse(docScrollWidth, out var w1) && w1 > 0)
                //    contentWidth = w1;

                var bodyScrollWidth = await webView.ExecuteScriptAsync("document.body.scrollWidth.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.body.scrollWidth " + bodyScrollWidth);
                //if (double.TryParse(bodyScrollWidth, out var w2) && w2 > 0 && w2 > contentWidth)
                //    contentWidth = w2;



                var clientRectWidth = await webView.ExecuteScriptAsync("document.documentElement.getBoundingClientRect().width.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.getBoundingClientRect().width " + clientRectWidth);
                var clientWidth = await webView.ExecuteScriptAsync("document.documentElement.clientWidth.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.clientWidth " + clientWidth);
                var innerWidth = await webView.ExecuteScriptAsync("self.innerWidth.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. self.innerWidth " + innerWidth);
                var offsetWidth = await webView.ExecuteScriptAsync("document.body.offsetWidth.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.body.offsetWidth " + offsetWidth);


                System.Diagnostics.Debug.WriteLine("elementHeight = " + webView.Height);

                //var rect = await webView.InvokeScriptAsync("pizzx", new[] { "document.getElementById( 'rasta' ).clientHeight.toString()" });
                // ask the content its height
                var docScrollHeight = await webView.ExecuteScriptAsync("document.documentElement.scrollHeight.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.scrollHeight " + docScrollHeight);
                
                var bodyScrollHeight = await webView.ExecuteScriptAsync("document.body.scrollHeight.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.body.scrollHeight " + bodyScrollHeight);
                var clientRectHeight = await webView.ExecuteScriptAsync("document.documentElement.getBoundingClientRect().height.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.getBoundingClientRect().height " + clientRectHeight);
                var clientHeight = await webView.ExecuteScriptAsync("document.documentElement.clientHeight.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.documentElement.clientHeight " + clientHeight);
                var innerHeight = await webView.ExecuteScriptAsync("self.innerHeight.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. self.innerHeight " + innerHeight);
                var offsetHeight = await webView.ExecuteScriptAsync("document.body.offsetHeight.toString()");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions. document.body.offsetHeight " + offsetHeight);
                
                //var heightString = await webView.ExecuteScriptAsync("Math.max(document.body.scrollHeight, document.body.offsetHeight, document.documentElement.clientHeight, document.documentElement.scrollHeight ).toString()" });//, document.documentElement.offsetHeight ).toString()" });
                var heightString = docScrollHeight;
                double.TryParse(docScrollHeight, out contentHeight);
                */
            }
            catch (Exception e)
            {
                //await Forms9Patch.Debug.RequestUserHelp(e, "line = " + line + ", callerName=["+callerName+"]");
                System.Diagnostics.Debug.WriteLine("WebViewExtensions.WebViewContentSizeAsync FAIL: " + e.Message);
                Console.WriteLine("WebViewExtensions.WebViewContentSizeAsync FAIL: " + e.Message);
                return await WebViewContentSizeAsync(webView, depth + 1, callerName);
            }
            return new Windows.Foundation.Size(contentWidth, contentHeight);
        }

        /// <summary>
        /// Get content of a WebView as HTML
        /// </summary>
        /// <param name="webView"></param>
        /// <returns></returns>
        public static async Task<string> GetHtml(this BaseWebView webView)
        {
#if __P42WASM__ || !NET7_0
            var html = await webView.ExecuteScriptAsync("document.documentElement.outerHTML;");
            return html;
#else
            return await Task.FromResult(string.Empty);
#endif
        }

#if HAS_UNO
        public static async Task<string> ExecuteScriptAsync(this BaseWebView webView, string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                return await Task.FromResult(string.Empty);

#if __ANDROID__
            if (webView.GetAndroidWebView() is Android.Webkit.WebView droidWebView)
            {
                var javaResult = await droidWebView.EvaluateJavaScriptAsync(script);
                return javaResult?.ToString() ?? string.Empty;
            }
            return await Task.FromResult(string.Empty);

#elif __IOS__ || __MACCATALYST__ || __MACOS__
            if (webView.GetNativeWebView() is Microsoft.UI.Xaml.Controls.NativeWebView wkWebView)
            {
                var result = await wkWebView.EvaluateJavascriptAsync(CancellationToken.None, script);
                return result?.ToString() ?? string.Empty;
            }
            return await Task.FromResult(string.Empty);

#elif __P42WASM__
            return await webView._nativeWebView.InvokeScriptAsync(script);

#else
            throw new NotSupportedException();

#endif

        }
#endif
    }
}
