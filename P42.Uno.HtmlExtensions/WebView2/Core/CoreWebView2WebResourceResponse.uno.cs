using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Storage.Streams;
using Uno;

namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2WebResourceResponse : IEquatable<CoreWebView2WebResourceResponse>
    {
        static uint _instances;
        uint _instance = _instances++;


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IRandomAccessStream Content { get; set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2HttpResponseHeaders Headers { get; private set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public string ReasonPhrase { get; set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public int StatusCode { get; set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        internal CoreWebView2WebResourceResponse() { }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator ==(CoreWebView2WebResourceResponse x, CoreWebView2WebResourceResponse y)
        {
            return x?._instance == y?._instance;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator !=(CoreWebView2WebResourceResponse x, CoreWebView2WebResourceResponse y)
        {
            return !(x == y);
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Equals(CoreWebView2WebResourceResponse other)
        {
            return this == other;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override bool Equals(object obj)
        {
            CoreWebView2WebResourceResponse coreWebView2WebResourceResponse = obj as CoreWebView2WebResourceResponse;
            if ((object)coreWebView2WebResourceResponse != null)
            {
                return this == coreWebView2WebResourceResponse;
            }

            return false;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


    }
}
