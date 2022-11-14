using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Uno;

namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2AcceleratorKeyPressedEventArgs : IEquatable<CoreWebView2AcceleratorKeyPressedEventArgs>
    {
        static uint _instances;
        uint _instance = _instances++;


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Handled { get; set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2KeyEventKind KeyEventKind { get; internal set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public int KeyEventLParam { get; internal set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2PhysicalKeyStatus PhysicalKeyStatus { get; internal set; }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public uint VirtualKey { get; internal set; }

        internal CoreWebView2AcceleratorKeyPressedEventArgs()
        {
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator ==(CoreWebView2AcceleratorKeyPressedEventArgs x, CoreWebView2AcceleratorKeyPressedEventArgs y)
        {
            return x?._instance == y?._instance;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator !=(CoreWebView2AcceleratorKeyPressedEventArgs x, CoreWebView2AcceleratorKeyPressedEventArgs y)
        {
            return !(x == y);
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Equals(CoreWebView2AcceleratorKeyPressedEventArgs other)
        {
            return this == other;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override bool Equals(object obj)
        {
            CoreWebView2AcceleratorKeyPressedEventArgs coreWebView2AcceleratorKeyPressedEventArgs = obj as CoreWebView2AcceleratorKeyPressedEventArgs;
            if ((object)coreWebView2AcceleratorKeyPressedEventArgs != null)
            {
                return this == coreWebView2AcceleratorKeyPressedEventArgs;
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
