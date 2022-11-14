using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Uno;

namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2ProcessInfo : IEquatable<CoreWebView2ProcessInfo>
    {
        static uint _instances;
        uint _instance = _instances++;


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2ProcessKind Kind { get; internal set; }

        public int ProcessId { get; internal set; }

        internal CoreWebView2ProcessInfo() {}

        public static bool operator ==(CoreWebView2ProcessInfo x, CoreWebView2ProcessInfo y)
        {
            return x?._instance == y?._instance;
        }

        public static bool operator !=(CoreWebView2ProcessInfo x, CoreWebView2ProcessInfo y)
        {
            return !(x == y);
        }

        public bool Equals(CoreWebView2ProcessInfo other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            CoreWebView2ProcessInfo coreWebView2ProcessInfo = obj as CoreWebView2ProcessInfo;
            if ((object)coreWebView2ProcessInfo != null)
            {
                return this == coreWebView2ProcessInfo;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
