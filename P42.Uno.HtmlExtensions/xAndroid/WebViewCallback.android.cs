using Android.Graphics;
using Android.Views;
using System;
using System.Threading.Tasks;

namespace P42.Uno.HtmlExtensions.Droid;

internal class WebViewCallBack : Android.Webkit.WebViewClient
{
    private bool _complete;
    private readonly string _jobName;
    private readonly PageSize _pageSize;
    private readonly PageMargin _margin;
    private readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
    private readonly Func<Android.Webkit.WebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> _onPageFinished;

    public WebViewCallBack(TaskCompletionSource<ToFileResult> taskCompletionSource, string jobName, PageSize pageSize, PageMargin margin, Func<Android.Webkit.WebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> onPageFinished)
    {
        _jobName = jobName;
        _pageSize = pageSize;
        _margin = margin;
        _taskCompletionSource = taskCompletionSource;
        _onPageFinished = onPageFinished;
    }

    public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
    {
        System.Diagnostics.Debug.WriteLine($"{nameof(WebViewCallBack)}OnPageStarted: ");
        base.OnPageStarted(view, url, favicon);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "Needed to invoke async code on main thread.")]
    public override void OnPageFinished(Android.Webkit.WebView view, string url)
    {
        System.Diagnostics.Debug.WriteLine($"{nameof(WebViewCallBack)}OnPageFinished: SUCCESS!");
        if (_complete)
            return;

        _complete = true;
        MainThread.BeginInvokeOnMainThread(() => 
        {
            view?.SetWebViewClient(null);
            _onPageFinished?.Invoke(view, _jobName, _pageSize, _margin, _taskCompletionSource); 
        });
    }

    public override void OnReceivedError(Android.Webkit.WebView view, Android.Webkit.IWebResourceRequest request, Android.Webkit.WebResourceError error)
    {
        base.OnReceivedError(view, request, error);

        if (error.Description == "net::ERR_CLEARTEXT_NOT_PERMITTED" ||
            error.Description == "net::ERR_FILE_NOT_FOUND")
            return;
        
        var url = view.Url;
        if (url == null || url != request.Url.ToString())
            return;

        System.Diagnostics.Debug.WriteLine($"WebViewCallBack.OnReceivedError : {error?.Description} {request.Url}");
        view.SetWebViewClient(null);
        _taskCompletionSource?.TrySetResult(new ToFileResult(error.Description));
    }

    public override void OnReceivedHttpError(Android.Webkit.WebView view, Android.Webkit.IWebResourceRequest request, Android.Webkit.WebResourceResponse errorResponse)
    {
        base.OnReceivedHttpError(view, request, errorResponse);

        var url = view?.Url;
        if (url == null || url != request.Url.ToString())
            return;

        System.Diagnostics.Debug.WriteLine($"WebViewCallBack.OnReceivedError : {errorResponse?.ReasonPhrase} {request.Url}");
        view?.SetWebViewClient(null);
        _taskCompletionSource?.TrySetResult(new ToFileResult(errorResponse.ReasonPhrase));
    }

    public override bool OnRenderProcessGone(Android.Webkit.WebView view, Android.Webkit.RenderProcessGoneDetail detail)
    {
        System.Diagnostics.Debug.WriteLine($"{nameof(WebViewCallBack)}OnRenderProcessGone: ");
        return base.OnRenderProcessGone(view, detail);
    }

    public override void OnLoadResource(Android.Webkit.WebView view, string url)
    {
        System.Diagnostics.Debug.WriteLine($"{nameof(WebViewCallBack)}OnLoadResource: ");
        base.OnLoadResource(view, url);
        Timer.StartTimer(TimeSpan.FromSeconds(10), () =>
        {
            if (!_complete)
                OnPageFinished(view, url);
            return false;
        });
    }

    public override void OnPageCommitVisible(Android.Webkit.WebView view, string url)
    {
        System.Diagnostics.Debug.WriteLine($"{nameof(WebViewCallBack)}OnPageCommitVisible: ");
        base.OnPageCommitVisible(view, url);
    }

    public override void OnUnhandledKeyEvent(Android.Webkit.WebView view, KeyEvent e)
    {
        System.Diagnostics.Debug.WriteLine($"{nameof(WebViewCallBack)}OnUnhandledKeyEvent: ");
        base.OnUnhandledKeyEvent(view, e);
    }

    public override void OnUnhandledInputEvent(Android.Webkit.WebView view, InputEvent e)
    {
        System.Diagnostics.Debug.WriteLine($"{nameof(WebViewCallBack)}OnUnhandledInputEvent: ");
        base.OnUnhandledInputEvent(view, e);
    }
}
