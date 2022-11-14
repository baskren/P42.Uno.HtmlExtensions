using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.UI.Core;
using Uno;

namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2ControllerWindowReference : IEquatable<CoreWebView2ControllerWindowReference>
    {
        static uint _instances;
        uint _instance = _instances++;


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWindow CoreWindow { get; internal set; }

        public ulong WindowHandle { get; internal set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static CoreWebView2ControllerWindowReference CreateFromWindowHandle(ulong windowHandle)
        {
            return new CoreWebView2ControllerWindowReference
            {
                WindowHandle = windowHandle,
            };

        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static CoreWebView2ControllerWindowReference CreateFromCoreWindow(CoreWindow coreWindow)
        {
            return new CoreWebView2ControllerWindowReference
            {
                CoreWindow = coreWindow
            };
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        internal CoreWebView2ControllerWindowReference() { }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator ==(CoreWebView2ControllerWindowReference x, CoreWebView2ControllerWindowReference y)
        {
            return x?._instance == y?._instance;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator !=(CoreWebView2ControllerWindowReference x, CoreWebView2ControllerWindowReference y)
        {
            return !(x == y);
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Equals(CoreWebView2ControllerWindowReference other)
        {
            return this == other;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override bool Equals(object obj)
        {
            CoreWebView2ControllerWindowReference coreWebView2ControllerWindowReference = obj as CoreWebView2ControllerWindowReference;
            if ((object)coreWebView2ControllerWindowReference != null)
            {
                return this == coreWebView2ControllerWindowReference;
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
