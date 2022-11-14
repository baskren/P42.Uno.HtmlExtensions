using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Uno;

namespace P42.Web.WebView2.Core
{
    [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
    public class CoreWebView2ProcessFailedEventArgs 
    {

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public int ExitCode { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public IReadOnlyList<P42.Web.WebView2.Core.CoreWebView2FrameInfo> FrameInfosForFailedProcess { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public string ProcessDescription { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedKind ProcessFailedKind { get; internal set; } = Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedKind.UnknownProcessExited;

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedReason Reason { get; internal set; } = Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedReason.Unexpected;

        internal CoreWebView2ProcessFailedEventArgs() {}
    }
}