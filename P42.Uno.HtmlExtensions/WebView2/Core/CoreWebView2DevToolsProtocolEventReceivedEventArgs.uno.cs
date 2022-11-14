using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Uno;

namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2DevToolsProtocolEventReceivedEventArgs : IEquatable<CoreWebView2DevToolsProtocolEventReceivedEventArgs>
    {
        static uint _instances;
        uint _instance = _instances++;


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public string ParameterObjectAsJson => string.Empty;

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public string SessionId => string.Empty;

        internal CoreWebView2DevToolsProtocolEventReceivedEventArgs() {}

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator ==(CoreWebView2DevToolsProtocolEventReceivedEventArgs x, CoreWebView2DevToolsProtocolEventReceivedEventArgs y)
        {
            return x?._instance == y?._instance;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator !=(CoreWebView2DevToolsProtocolEventReceivedEventArgs x, CoreWebView2DevToolsProtocolEventReceivedEventArgs y)
        {
            return !(x == y);
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Equals(CoreWebView2DevToolsProtocolEventReceivedEventArgs other)
        {
            return this == other;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override bool Equals(object obj)
        {
            CoreWebView2DevToolsProtocolEventReceivedEventArgs coreWebView2DevToolsProtocolEventReceivedEventArgs = obj as CoreWebView2DevToolsProtocolEventReceivedEventArgs;
            if ((object)coreWebView2DevToolsProtocolEventReceivedEventArgs != null)
            {
                return this == coreWebView2DevToolsProtocolEventReceivedEventArgs;
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
