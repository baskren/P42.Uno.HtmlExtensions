using System;
using Uno;

namespace P42.Web.WebView2.Core
{

    public class CoreWebView2NavigationCompletedEventArgs
    {
        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public bool IsSuccess { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public ulong NavigationId { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public Microsoft.Web.WebView2.Core.CoreWebView2WebErrorStatus WebErrorStatus { get; internal set; }

        internal CoreWebView2NavigationCompletedEventArgs() {}
    }
}
