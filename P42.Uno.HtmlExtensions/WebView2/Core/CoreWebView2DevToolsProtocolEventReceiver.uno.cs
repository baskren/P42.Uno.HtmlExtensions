using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Uno;
using Windows.Foundation;

namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2DevToolsProtocolEventReceiver : IEquatable<CoreWebView2DevToolsProtocolEventReceiver>
    {
        static uint _instances;
        uint _instance = _instances++;


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public event TypedEventHandler<CoreWebView2, CoreWebView2DevToolsProtocolEventReceivedEventArgs> DevToolsProtocolEventReceived;

        internal CoreWebView2DevToolsProtocolEventReceiver() {}

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator ==(CoreWebView2DevToolsProtocolEventReceiver x, CoreWebView2DevToolsProtocolEventReceiver y)
        {
            return x?._instance == y?._instance;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator !=(CoreWebView2DevToolsProtocolEventReceiver x, CoreWebView2DevToolsProtocolEventReceiver y)
        {
            return !(x == y);
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Equals(CoreWebView2DevToolsProtocolEventReceiver other)
        {
            return this == other;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override bool Equals(object obj)
        {
            CoreWebView2DevToolsProtocolEventReceiver coreWebView2DevToolsProtocolEventReceiver = obj as CoreWebView2DevToolsProtocolEventReceiver;
            if ((object)coreWebView2DevToolsProtocolEventReceiver != null)
            {
                return this == coreWebView2DevToolsProtocolEventReceiver;
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
