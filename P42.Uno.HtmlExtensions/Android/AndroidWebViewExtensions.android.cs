using Android.Runtime;
using Android.Views;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace P42.Uno.HtmlExtensions
{
    static class AndroidWebViewExtensions
    {
        public static int ContentWidth(this Android.Webkit.WebView webView)
        {
            var method = webView.GetType().GetMethod("ComputeHorizontalScrollRange", BindingFlags.NonPublic | BindingFlags.Instance);
            var width = (int)method.Invoke(webView, new object[] { });
            return width;
        }

        public static int ContentHeight(this Android.Webkit.WebView webView)
        {
            var method = webView.GetType().GetMethod("ComputeVerticalScrollRange", BindingFlags.NonPublic | BindingFlags.Instance);
            var height = (int)method.Invoke(webView, new object[] { });


            return (int)(height / Display.Scale) + webView.MeasuredHeight;
        }

        public static async Task<Java.Lang.Object> EvaluateJavaScriptAsync(this Android.Webkit.WebView webView, string script)
        {
            using (var evaluator = new JavaScriptEvaluator(webView, script))
            {
                return await evaluator.TaskCompletionSource.Task;
            }
        }
    }

    class JavaScriptEvaluator : Java.Lang.Object, Android.Webkit.IValueCallback
    {
        public TaskCompletionSource<Java.Lang.Object> TaskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();

        public JavaScriptEvaluator(Android.Webkit.WebView webView, string script)
        {
            webView.EvaluateJavascript(script, this);
        }
        public void OnReceiveValue(Java.Lang.Object value)
            => TaskCompletionSource.SetResult(value);

    }
}
