﻿using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.WebUI;

#if __WASM__
using WebView = P42.Uno.HtmlExtensions.WebViewX;
#else
using WebView = Microsoft.UI.Xaml.Controls.WebView2;
#endif


namespace P42.Uno.HtmlExtensions
{
    public static partial class WebViewExtensions
    {

        public static void NavigateToResource(this WebView webView, string resourceId, Assembly assembly)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceId))
            {
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd();
                    var path = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, $"{Guid.NewGuid()}.html");
                    File.WriteAllText(path, text);
                    webView.Source = new Uri($"file://{path}");
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
        
        static async Task<TryResult<int>> TryExecuteIntScriptAsync(this WebView2 webView2, string script)
        {
            var result = await webView2.ExecuteScriptAsync(script);
            if (int.TryParse(result, out int v))
                return new TryResult<int>(true,v);

            return new TryResult<int>(false);
        }

        static async Task<TryResult<double>> TryExecuteDoubleScriptAsync(this WebView2 webView2, string script)
        {
            var result = await webView2.ExecuteScriptAsync(script);
            System.Diagnostics.Debug.WriteLine($"WebViewExtensions.TryExecuteDoubleScriptAsync : [{script}] : [{result}]");
            if (double.TryParse(result, out var v))
                return new TryResult<double>(true, v);

            return new TryResult<double>(false);
        }

        static async Task<double> TryUpdateIfLarger(this WebView2 webView2, string script, double source)
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
        public static async Task<Windows.Foundation.Size> WebViewContentSizeAsync(this Microsoft.UI.Xaml.Controls.WebView2 webView, int depth = 0, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
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
                return await WebViewContentSizeAsync(webView, depth + 1, callerName);
            }
            return new Windows.Foundation.Size(contentWidth, contentHeight);
        }

        /// <summary>
        /// Get content of a WebView as HTML
        /// </summary>
        /// <param name="webView"></param>
        /// <returns></returns>
        public static async Task<string> GetHtml(this Microsoft.UI.Xaml.Controls.WebView2 webView)
        {
            var html = await webView.ExecuteScriptAsync("document.documentElement.outerHTML;");
            return html;
        }
    }
}
