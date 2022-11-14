using System;
using System.Runtime.InteropServices;
using System.Threading;
using Uno;
using Windows.Foundation;
using Windows.UI.Core;

namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2CompositionController : CoreWebView2Controller, IEquatable<CoreWebView2CompositionController>
    {
        static uint _instances;
        uint _instance = _instances++;

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreCursor Cursor => default;

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public object RootVisualTarget { get; set;}

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public event TypedEventHandler<CoreWebView2CompositionController, object> CursorChanged;

        internal CoreWebView2CompositionController() {}

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator ==(CoreWebView2CompositionController x, CoreWebView2CompositionController y)
        {
            return x?._instance == y?._instance;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator !=(CoreWebView2CompositionController x, CoreWebView2CompositionController y)
        {
            return !(x == y);
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Equals(CoreWebView2CompositionController other)
        {
            return this == other;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override bool Equals(object obj)
        {
            CoreWebView2CompositionController coreWebView2CompositionController = obj as CoreWebView2CompositionController;
            if ((object)coreWebView2CompositionController != null)
            {
                return this == coreWebView2CompositionController;
            }

            return false;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void SendMouseInput(CoreWebView2MouseEventKind eventKind, CoreWebView2MouseEventVirtualKeys virtualKeys, uint mouseData, Point point)
        {
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void SendPointerInput(CoreWebView2PointerEventKind eventKind, CoreWebView2PointerInfo pointerInfo)
        {
        }

    }
}
