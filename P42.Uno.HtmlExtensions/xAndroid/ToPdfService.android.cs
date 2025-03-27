#if __ANDROID__
using System.IO;
using Android.Graphics;
using Android.Views;
using System.Threading.Tasks;
using System.Reflection;
using Android.Print;
using Android.Runtime;
using System;
using Android.OS;
using Java.Lang;
using Java.Interop;
using P42.Uno.HtmlExtensions.Droid;
using P42.Uno.HtmlExtensions;
//using Uno.UI;
using System.Linq;
using Windows.Storage;
using Android.Text;

namespace P42.Uno.HtmlExtensions
{
    internal class NativeToPdfService : Java.Lang.Object, INativeToPdfService
    {
        /// <summary>
        /// Is to PDF conversion available?
        /// </summary>
        public bool IsAvailable => Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;


        public async Task<ToFileResult> ToPdfAsync(Uri uri, string fileName, PageSize pageSize, PageMargin margin)
        {
            var droidWebView = new Android.Webkit.WebView(Android.App.Application.Context);
            droidWebView.Settings.AllowFileAccess = true;
            droidWebView.Settings.AllowFileAccessFromFileURLs = true;
            droidWebView.Settings.AllowUniversalAccessFromFileURLs = true;
            droidWebView.Settings.AllowContentAccess = true;
            droidWebView.Settings.JavaScriptEnabled = true;
            droidWebView.Settings.DomStorageEnabled = true;

            if (Build.VERSION.SdkInt < (BuildVersionCodes)28)
#pragma warning disable CA1422 // Validate platform compatibility
                droidWebView.DrawingCacheEnabled = true;
#pragma warning restore CA1422 // Validate platform compatibility

            droidWebView.SetLayerType(LayerType.Software, null);
            droidWebView.Layout(0, 0, (int)System.Math.Ceiling(pageSize.Width), (int)System.Math.Ceiling(pageSize.Height));

            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
            using var callback = new WebViewCallBack(taskCompletionSource, fileName, pageSize, margin,
                OnPageFinishedAsync);
            ToFileResult result = null;
            try
            {
                droidWebView.SetWebViewClient(callback);
                droidWebView.LoadUrl(uri.AbsoluteUri);
                result = await taskCompletionSource.Task;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ToPdfService. : ");
                result = new ToFileResult(ex.Message);
            }
            System.Diagnostics.Debug.WriteLine($"DONE: ToPdfAsync [{fileName}]");
            callback.Dispose();
            droidWebView.Dispose();
            return result;
        }

        /// <summary>
        /// Convert HTLM to PDF
        /// </summary>
        /// <param name="html"></param>
        /// <param name="fileName"></param>
        /// <param name="pageSize"></param>
        /// <param name="margin"></param>
        /// <returns></returns>
        public async Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin)
        {
            var uri = await html.ToTempFileUriAsync();
            return await ToPdfAsync(uri, fileName, pageSize, margin);
        }

        /// <summary>
        /// Convert current content of WebView to PDF
        /// </summary>
        /// <param name="webView2"></param>
        /// <param name="fileName"></param>
        /// <param name="pageSize"></param>
        /// <param name="margin"></param>
        /// <returns></returns>
        public async Task<ToFileResult> ToPdfAsync(WebView2 webView2, string fileName, PageSize pageSize, PageMargin margin)
        {
            if (webView2.GetAndroidWebView() is { } droidWebView)
                return await ToPdfAsync(droidWebView, fileName, pageSize, margin);

            return await Task.FromResult(new ToFileResult("Could not get NativeWebView for Uno WebView"));
        }


        /// <summary>
        /// Convert current content of WebView to PDF
        /// </summary>
        /// <param name="webView"></param>
        /// <param name="fileName"></param>
        /// <param name="pageSize"></param>
        /// <param name="margin"></param>
        /// <returns></returns>
        public async Task<ToFileResult> ToPdfAsync(WebView webView, string fileName, PageSize pageSize, PageMargin margin)
        {
            if (webView.GetAndroidWebView() is { } droidWebView)
                return await ToPdfAsync(droidWebView, fileName, pageSize, margin);

            return await Task.FromResult(new ToFileResult("Could not get NativeWebView for Uno WebView"));
        }

        private async Task<ToFileResult> ToPdfAsync(Android.Webkit.WebView droidWebView, string fileName, PageSize pageSize, PageMargin margin)
        {
            droidWebView.SetLayerType(LayerType.Software, null);
            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();

            await OnPageFinishedAsync(droidWebView, fileName, pageSize, margin, taskCompletionSource);
            
            return await taskCompletionSource.Task;
        }

        private static async Task OnPageFinishedAsync(Android.Webkit.WebView webView, string fileName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                return;
            
            await Task.Delay(5);
            using var mediaSize = new PrintAttributes.MediaSize(pageSize.Name, pageSize.Name, (int)(pageSize.Width * 1000 / 72), (int)(pageSize.Height * 1000 / 72));
            using var resolution = new PrintAttributes.Resolution("pdf", "pdf", 72, 72);
            using var builder = new PrintAttributes.Builder()
                                    .SetMediaSize(mediaSize)
                                    .SetResolution(resolution);
            
            PrintAttributes attributes;
            if (margin is null)
            {
                builder.SetMinMargins(PrintAttributes.Margins.NoMargins);
                attributes = builder.Build();
            }
            else
            {
                using var margins = new PrintAttributes.Margins((int)(margin.Left * 1000 / 72), (int)(margin.Top * 1000 / 72), (int)(margin.Right * 1000 / 72), (int)(margin.Bottom * 1000 / 72));
                builder.SetMinMargins(margins);
                attributes = builder.Build();
            }
                        
            var adapter = webView.CreatePrintDocumentAdapter(Guid.NewGuid().ToString());
            using var layoutResultCallback = new PdfLayoutResultCallback();
            layoutResultCallback.Adapter = adapter;
            layoutResultCallback.TaskCompletionSource = taskCompletionSource;
            layoutResultCallback.FileName = fileName;
            adapter.OnLayout(null, attributes, null, layoutResultCallback, null);
            await taskCompletionSource.Task;
            
            
        }
    }

}


namespace Android.Print
{
    [Register("android/print/PdfLayoutResultCallback")]
    public class PdfLayoutResultCallback : PrintDocumentAdapter.LayoutResultCallback
    {
        public TaskCompletionSource<ToFileResult> TaskCompletionSource { get; set; }
        public string FileName { get; set; }
        public PrintDocumentAdapter Adapter { get; set; }

        public PdfLayoutResultCallback(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public PdfLayoutResultCallback() : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            if (Handle != IntPtr.Zero)
                return;
            
            unsafe
            {
                var val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
            }
        }

        public override void OnLayoutCancelled()
        {
            base.OnLayoutCancelled();
            System.Diagnostics.Debug.WriteLine($"PdfLayoutResultCallback.OnLayoutCancelled [{FileName}]");
            TaskCompletionSource.SetResult(new ToFileResult("PDF Layout was cancelled"));
        }

        public override void OnLayoutFailed(ICharSequence error)
        {
            base.OnLayoutFailed(error);
            System.Diagnostics.Debug.WriteLine($"PdfLayoutResultCallback.OnLayoutFailed [{FileName}]");
            TaskCompletionSource.SetResult(new ToFileResult(error.ToString()));
        }

        public override void OnLayoutFinished(PrintDocumentInfo info, bool changed)
        {
            var dir = App.Application.Context.CacheDir;
            if (!dir.Exists())
                dir.Mkdir();

            var suffix = System.IO.Path.GetExtension(FileName);
            FileName = System.IO.Path.GetFileNameWithoutExtension(FileName);
            var file = Java.IO.File.CreateTempFile($"{FileName}.", suffix, dir);
            var fileDescriptor = ParcelFileDescriptor.Open(file, ParcelFileMode.ReadWrite);
            Adapter.OnWrite([PageRange.AllPages], 
                fileDescriptor, new CancellationSignal(), 
                new PdfWriteResultCallback(TaskCompletionSource, file)
                );
            base.OnLayoutFinished(info, changed);
            System.Diagnostics.Debug.WriteLine($"PdfLayoutResultCallback.OnLayoutFinished [{FileName}]");
        }


    }

    [Register("android/print/PdfWriteResult")]
    public class PdfWriteResultCallback : PrintDocumentAdapter.WriteResultCallback
    {
        private readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
        //readonly string _path;
        private readonly Java.IO.File _file;

        public PdfWriteResultCallback(TaskCompletionSource<ToFileResult> taskCompletionSource, Java.IO.File file, IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            _taskCompletionSource = taskCompletionSource;
            _file = file;
        }

        public PdfWriteResultCallback(TaskCompletionSource<ToFileResult> taskCompletionSource, Java.IO.File file) : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            if (Handle == IntPtr.Zero)
            {
                unsafe
                {
                    var val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                    SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                    JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                }
            }
            _taskCompletionSource = taskCompletionSource;
            _file = file;
        }


        public override void OnWriteFinished(PageRange[] pages)
        {
            base.OnWriteFinished(pages);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(_file.AbsolutePath);
                System.Diagnostics.Debug.WriteLine($"PdfWriteResultCallback.OnWriteFinished [{_file.Name}]");
                _taskCompletionSource.SetResult(new ToFileResult(storageFile));
                _file.Dispose();
            });
        }

        public override void OnWriteCancelled()
        {
            base.OnWriteCancelled();
            System.Diagnostics.Debug.WriteLine($"PdfWriteResultCallback.OnWriteCancelled [{_file.Name}]");
            _taskCompletionSource.SetResult(new ToFileResult("PDF Write was cancelled"));
            _file.Dispose();
        }

        public override void OnWriteFailed(ICharSequence error)
        {
            base.OnWriteFailed(error);
            System.Diagnostics.Debug.WriteLine($"PdfWriteResultCallback.OnWriteFailed [{_file.Name}]");
            _taskCompletionSource.SetResult(new ToFileResult(error?.ToString() ?? "PDF File Write Failed"));
            _file.Dispose();
        }
    }


}
#endif
