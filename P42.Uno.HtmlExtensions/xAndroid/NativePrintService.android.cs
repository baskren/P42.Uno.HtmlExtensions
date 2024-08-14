#if __ANDROID__
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Views;
using P42.Uno.HtmlExtensions.Droid;
//using Uno.UI;
using Microsoft.UI.Xaml.Controls;
using static Android.Icu.Util.LocaleData;

namespace P42.Uno.HtmlExtensions
{
    class NativePrintService : INativePrintService
    {
        // internal static readonly Java.Lang.String GeneratedTag = new Java.Lang.String("P42_GENERATED");
        public bool IsAvailable => Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

        public void AllowFileAccess(WebView2 webView, bool allow)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                return;

            if (webView.GetAndroidWebView() is Android.Webkit.WebView droidWebView)
            {
                droidWebView.Settings.AllowFileAccess = allow;
                droidWebView.Settings.AllowFileAccessFromFileURLs = allow;
                droidWebView.Settings.AllowUniversalAccessFromFileURLs = allow;
            }
            else
                throw new Exception("Cannot find Android.Webkit.WebView for WebView2");
        }

        public void AllowNetworkLoads(WebView2 webView, bool allow)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                return;

            if (webView.GetAndroidWebView() is Android.Webkit.WebView droidWebView)
            {
                droidWebView.Settings.AllowContentAccess = !allow;
                droidWebView.Settings.BlockNetworkImage = !allow;
                droidWebView.Settings.BlockNetworkLoads = !allow;
            }
            else
                throw new Exception("Cannot find Android.Webkit.WebView for WebView2");
        }

        private static Android.App.Activity Activity => global::Uno.UI.ContextHelper.Current as Android.App.Activity;

        public async Task PrintAsync(WebView2 webView2, string jobName)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                throw new Exception("Cannot print for Android versions older than Kitkat");

            if (webView2.GetAndroidWebView() is Android.Webkit.WebView droidWebView)
                await PrintAsync(droidWebView, jobName);
            else
                throw new Exception("Cannot find Android.Webkit.WebView for WebView2");
        }

        public async Task PrintAsync(WebView webView, string jobName)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                throw new Exception("Cannot print for Android versions older than Kitkat");

            if (webView.GetAndroidWebView() is Android.Webkit.WebView droidWebView)
                await PrintAsync(droidWebView, jobName);
            else
                throw new Exception("Cannot find Android.Webkit.WebView for WebView2");
        }

        private static async Task PrintAsync(Android.Webkit.WebView droidWebView, string jobName)
        {
            droidWebView.Settings.JavaScriptEnabled = true;
            droidWebView.Settings.DomStorageEnabled = true;
            droidWebView.SetLayerType(Android.Views.LayerType.Software, null);

            // Only valid for API 19+
            if (string.IsNullOrWhiteSpace(jobName))
            {
                var javaResult = await droidWebView.EvaluateJavaScriptAsync("document.title");
                jobName = javaResult?.ToString();
            }
            if (string.IsNullOrWhiteSpace(jobName))
                jobName = AppInfo.Name;

            var printMgr = (PrintManager)Activity.GetSystemService(Context.PrintService);
            printMgr.Print(jobName, droidWebView.CreatePrintDocumentAdapter(jobName), null);

            await Task.CompletedTask;
        }

        public async Task PrintAsync(string html, string jobName)
        {
            var uri = await html.ToTempFileUriAsync();
            await uri.PrintAsync(jobName);
        }

        public async Task PrintAsync(Uri uri, string jobName)
        {

            var droidWebView = new Android.Webkit.WebView(Android.App.Application.Context);
            droidWebView.Settings.AllowFileAccess = true;
            droidWebView.Settings.AllowFileAccessFromFileURLs = true;
            droidWebView.Settings.AllowUniversalAccessFromFileURLs = true;
            droidWebView.Settings.AllowContentAccess = true;
            droidWebView.Settings.DomStorageEnabled = true;
            droidWebView.Settings.JavaScriptEnabled = true;
            droidWebView.Settings.BlockNetworkImage = false;
            droidWebView.Settings.BlockNetworkLoads = false;

            if (Android.OS.Build.VERSION.SdkInt < (Android.OS.BuildVersionCodes)28)
#pragma warning disable CA1422 // Validate platform compatibility
                droidWebView.DrawingCacheEnabled = true;
#pragma warning restore CA1422 // Validate platform compatibility

            droidWebView.SetLayerType(LayerType.Software, null);
            droidWebView.Layout(36, 36, (int)((PageSize.Default.Width - 0.5) * 72), (int)((PageSize.Default.Height - 0.5) * 72));

            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
            using var callBack = new WebViewCallBack(taskCompletionSource, jobName,
                PageSize.Default, null, OnPageFinishedAsync);
            try
            {
                droidWebView.SetWebViewClient(callBack);
                droidWebView.LoadUrl(uri.AbsoluteUri);
                await taskCompletionSource.Task;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NativePrintService. : ");
                taskCompletionSource.SetResult(new ToFileResult(ex.Message));
            }
            callBack.Dispose();
            droidWebView.Dispose();
        }

        static async Task OnPageFinishedAsync(Android.Webkit.WebView webView, string jobName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                jobName = AppInfo.Name;
            var printMgr = (PrintManager)Activity.GetSystemService(Context.PrintService);
            printMgr.Print(jobName, webView.CreatePrintDocumentAdapter(jobName), null);
            taskCompletionSource.TrySetResult(new ToFileResult(storageFile: null));
            await Task.CompletedTask;
        }

    }

}
#endif
