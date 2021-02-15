#if __ANDROID__
using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Views;
using P42.Uno.HtmlExtensions.Droid;
using Uno.UI;
using Windows.UI.Xaml.Controls;

namespace P42.Uno.HtmlExtensions
{
    public class NativePrintService : INativePrintService
    {
        public bool IsAvailable() => Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

        public async Task PrintAsync(WebView unoWebView, string jobName)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                var context = Android.App.Application.Context;
                if (unoWebView.GetChildren(v => v is Android.Webkit.WebView).FirstOrDefault() is Android.Webkit.WebView droidWebView)
                {
                    droidWebView.Settings.JavaScriptEnabled = true;
                    droidWebView.Settings.DomStorageEnabled = true;
                    droidWebView.SetLayerType(Android.Views.LayerType.Software, null);

                    // Only valid for API 19+
                    if (string.IsNullOrWhiteSpace(jobName))
                        jobName = unoWebView.DocumentTitle;
                    if (string.IsNullOrWhiteSpace(jobName))
                        jobName = AppInfo.Name;
                    var printMgr = (PrintManager)context.GetSystemService(Context.PrintService);
                    printMgr.Print(jobName, droidWebView.CreatePrintDocumentAdapter(jobName), null);
                }
                else
                {
                    throw new Exception("Cannot find Android.Webkit.WebView for WebView");
                }
            }
            await Task.CompletedTask;
        }

        public async Task PrintAsync(string html, string jobName)
        {
            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
            using (var webView = new Android.Webkit.WebView(Android.App.Application.Context))
            {
                webView.Settings.JavaScriptEnabled = true;
                webView.Settings.DomStorageEnabled = true;
#pragma warning disable CS0618 // Type or member is obsolete
                webView.DrawingCacheEnabled = true;
#pragma warning restore CS0618 // Type or member is obsolete
                webView.SetLayerType(LayerType.Software, null);

                webView.Layout(36, 36, (int)((PageSize.Default.Width - 0.5) * 72), (int)((PageSize.Default.Height - 0.5) * 72));
                using (var webViewCallBack = new WebViewCallBack(taskCompletionSource, jobName, PageSize.Default, null, OnPageFinishedAsync))
                {
                    webView.SetWebViewClient(webViewCallBack);
                    webView.LoadData(html, "text/html; charset=utf-8", "UTF-8");
                    await taskCompletionSource.Task;
                }
            }
        }

        static async Task OnPageFinishedAsync(Android.Webkit.WebView webView, string jobName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                jobName = AppInfo.Name;
            var printMgr = (PrintManager)Android.App.Application.Context.GetSystemService(Context.PrintService);
            await Task.Delay(1000); // allow a bit more time for the layout to complete ... there has to be a better way to do this!?!?;
            printMgr.Print(jobName, webView.CreatePrintDocumentAdapter(jobName), null);
            taskCompletionSource.SetResult(new ToFileResult(storageFile: null));
        }
    }

}
#endif