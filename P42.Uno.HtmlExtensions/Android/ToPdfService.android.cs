﻿#if __ANDROID__
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
using Uno.UI;
using System.Linq;
using Windows.Storage;

namespace P42.Uno.HtmlExtensions
{

    public class NativeToPdfService : Java.Lang.Object, INativeToPdfService
    {
        public bool IsAvailable => Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;

        public async Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin)
        {
            using (var droidWebView = new Android.Webkit.WebView(Android.App.Application.Context))
            {
                droidWebView.Settings.JavaScriptEnabled = true;
#pragma warning disable CS0618 // Type or member is obsolete
                droidWebView.DrawingCacheEnabled = true;
#pragma warning restore CS0618 // Type or member is obsolete
                droidWebView.SetLayerType(LayerType.Software, null);
                droidWebView.Layout(0, 0, (int)System.Math.Ceiling(pageSize.Width), (int)System.Math.Ceiling(pageSize.Height));
                droidWebView.LoadData(html, "text/html; charset=utf-8", "UTF-8");

                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                using (var callback = new WebViewCallBack(taskCompletionSource, fileName, pageSize, margin, OnPageFinished))
                {
                    droidWebView.SetWebViewClient(callback);
                    return await taskCompletionSource.Task;
                }
            }
        }

        public async Task<ToFileResult> ToPdfAsync(Windows.UI.Xaml.Controls.WebView unoWebView, string fileName, PageSize pageSize, PageMargin margin)
        {
            if (unoWebView.GetChildren(v => v is Android.Webkit.WebView).FirstOrDefault() is Android.Webkit.WebView droidWebView)
            {
                droidWebView.SetLayerType(LayerType.Software, null);
                droidWebView.Settings.JavaScriptEnabled = true;
#pragma warning disable CS0618 // Type or member is obsolete
                droidWebView.DrawingCacheEnabled = true;
                droidWebView.BuildDrawingCache();
#pragma warning restore CS0618 // Type or member is obsolete
                var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
                using (var callback = new WebViewCallBack(taskCompletionSource, fileName, pageSize, margin, OnPageFinished))
                {
                    droidWebView.SetWebViewClient(callback);
                    return await taskCompletionSource.Task;
                }
            }
            return await Task.FromResult(new ToFileResult("Could not get NativeWebView for Uno WebView"));
        }


        static async Task OnPageFinished(Android.Webkit.WebView droidWebView, string fileName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            {
                await Task.Delay(5);
                using (var builder = new PrintAttributes.Builder())
                {
                    //builder.SetMediaSize(PrintAttributes.MediaSize.NaLetter);
                    using (var mediaSize = new PrintAttributes.MediaSize(pageSize.Name, pageSize.Name, (int)(pageSize.Width * 1000 / 72), (int)(pageSize.Height * 1000 / 72)))
                    {
                        builder.SetMediaSize(mediaSize);
                        using (var resolution = new PrintAttributes.Resolution("pdf", "pdf", 72, 72))
                        {
                            builder.SetResolution(resolution);
                            PrintAttributes attributes;
                            if (margin is null)
                            {
                                builder.SetMinMargins(PrintAttributes.Margins.NoMargins);
                                attributes = builder.Build();
                            }
                            else
                            {
                                using (var margins = new PrintAttributes.Margins((int)(margin.Left * 1000 / 72), (int)(margin.Top * 1000 / 72), (int)(margin.Right * 1000 / 72), (int)(margin.Bottom * 1000 / 72)))
                                {
                                    builder.SetMinMargins(margins);
                                    attributes = builder.Build();
                                }
                            }
                            
                            var adapter = droidWebView.CreatePrintDocumentAdapter(Guid.NewGuid().ToString());
                            using (var layoutResultCallback = new PdfLayoutResultCallback())
                            {
                                layoutResultCallback.Adapter = adapter;
                                layoutResultCallback.TaskCompletionSource = taskCompletionSource;
                                layoutResultCallback.FileName = fileName;
                                adapter.OnLayout(null, attributes, null, layoutResultCallback, null);
                                await taskCompletionSource.Task;
                            }
                        }
                    }
                }
            }
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
            if (!(Handle != IntPtr.Zero))
            {
                unsafe
                {
                    var val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                    SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                    JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                }
            }
        }

        public override void OnLayoutCancelled()
        {
            base.OnLayoutCancelled();
            TaskCompletionSource.SetResult(new ToFileResult("PDF Layout was cancelled"));
        }

        public override void OnLayoutFailed(ICharSequence error)
        {
            base.OnLayoutFailed(error);
            TaskCompletionSource.SetResult(new ToFileResult(error.ToString()));
        }

        public override void OnLayoutFinished(PrintDocumentInfo info, bool changed)
        {
            using (var _dir = Android.App.Application.Context.CacheDir)
            {
                if (!_dir.Exists())
                    _dir.Mkdir();

                var suffix = System.IO.Path.GetExtension(FileName);
                FileName = System.IO.Path.GetFileNameWithoutExtension(FileName);
                var file = Java.IO.File.CreateTempFile(FileName+".", suffix, _dir);
                var fileDescriptor = ParcelFileDescriptor.Open(file, ParcelFileMode.ReadWrite);
                var writeResultCallback = new PdfWriteResultCallback(TaskCompletionSource, file.AbsolutePath);

                using (var cancelSignal = new CancellationSignal())
                {
                    Adapter.OnWrite(new Android.Print.PageRange[] { PageRange.AllPages }, fileDescriptor, cancelSignal, writeResultCallback);
                    file.Dispose();
                }
            }
            base.OnLayoutFinished(info, changed);
        }


    }

    [Register("android/print/PdfWriteResult")]
    public class PdfWriteResultCallback : PrintDocumentAdapter.WriteResultCallback
    {
        readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
        readonly string _path;

        public PdfWriteResultCallback(TaskCompletionSource<ToFileResult> taskCompletionSource, string path, IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            _taskCompletionSource = taskCompletionSource;
            _path = path;
        }

        public PdfWriteResultCallback(TaskCompletionSource<ToFileResult> taskCompletionSource, string path) : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            if (!(Handle != IntPtr.Zero))
            {
                unsafe
                {
                    var val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                    SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                    JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                }
            }
            _taskCompletionSource = taskCompletionSource;
            _path = path;
        }


        public override void OnWriteFinished(PageRange[] pages)
        {
            base.OnWriteFinished(pages);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(_path);
                _taskCompletionSource.SetResult(new ToFileResult(storageFile));
            });
        }

        public override void OnWriteCancelled()
        {
            base.OnWriteCancelled();
            _taskCompletionSource.SetResult(new ToFileResult("PDF Write was cancelled"));
        }

        public override void OnWriteFailed(ICharSequence error)
        {
            base.OnWriteFailed(error);
            _taskCompletionSource.SetResult(new ToFileResult(error.ToString()));
        }
    }


}
#endif