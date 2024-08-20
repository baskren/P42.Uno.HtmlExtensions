using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using System.Reflection;

namespace Uno.UI.Tests;

public partial class WebView2 : Microsoft.UI.Xaml.Controls.WebView2
{
    public WebView2()
    {

    }

    protected override void OnApplyTemplate()
    {
        Debug.WriteLine($"WEBVIEW2 CONSTRUCTION : OnApplyTemplate=[]");
        Console.WriteLine($"WEBVIEW2 CONSTRUCTION : OnApplyTemplate=[]");

        base.OnApplyTemplate();
    }

    public void CheckConnection()
    {
        var _coreWebView = this.CoreWebView2;

        Debug.WriteLine($"WEBVIEW2 CONSTRUCTION : CoreWebView=[{_coreWebView}]");
        Console.WriteLine($"WEBVIEW2 CONSTRUCTION : CoreWebView=[{_coreWebView}]");

#if __WASM__

        var cwvType = _coreWebView.GetType();
        var nwvFieldInfo = cwvType.GetField("_nativeWebView", BindingFlags.Instance | BindingFlags.NonPublic);
        var _nativeWebView = nwvFieldInfo?.GetValue(_coreWebView);
        Debug.WriteLine($"WEBVIEW2 CONSTRUCTION : READ NativeWebView=[{_nativeWebView}]");
        Console.WriteLine($"WEBVIEW2 CONSTRUCTION : READ NativeWebView=[{_nativeWebView}]");

        //_nativeWebView = new global::Uno.UI.Tests.NativeWebView(_coreWebView);
        //nwvFieldInfo?.SetValue(_coreWebView, _nativeWebView);
        //Debug.WriteLine($"WEBVIEW2 CONSTRUCTION : WRITE NativeWebView=[{_nativeWebView}]");
        //Console.WriteLine($"WEBVIEW2 CONSTRUCTION : WRITE NativeWebView=[{_nativeWebView}]");

        _nativeWebView = nwvFieldInfo?.GetValue(_coreWebView);
        Debug.WriteLine($"WEBVIEW2 CONSTRUCTION : READ NativeWebView=[{_nativeWebView}]");
        Console.WriteLine($"WEBVIEW2 CONSTRUCTION : READ NativeWebView=[{_nativeWebView}]");

        var _coreWebView2InitializedFieldInfo = GetType().GetField("_coreWebView2Initialized", BindingFlags.Instance | BindingFlags.NonPublic);
        var _coreWebView2Initialized = _coreWebView2InitializedFieldInfo?.GetValue(this);
        Debug.WriteLine($"WEBVIEW2 CONSTRUCTION : READ _coreWebView2Initialized=[{_coreWebView2Initialized}]");
        Console.WriteLine($"WEBVIEW2 CONSTRUCTION : READ _coreWebView2Initialized=[{_coreWebView2Initialized}]");

        var _verifyWebViewAvailableMethod = cwvType.GetMethod("VerifyWebViewAvailability", BindingFlags.Instance | BindingFlags.NonPublic);
        var _verifyWebViewAvailable = _verifyWebViewAvailableMethod?.Invoke(_coreWebView, null);
        Debug.WriteLine($"WEBVIEW2 CONSTRUCTION : READ _verifyWebViewAvailable=[{_verifyWebViewAvailable}]");
        Console.WriteLine($"WEBVIEW2 CONSTRUCTION : READ _verifyWebViewAvailable=[{_verifyWebViewAvailable}]");



#endif

    }
}
