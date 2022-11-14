using System;
using Uno;
using Windows.Foundation.Metadata;

namespace P42.Web.WebView2.Core
{
    public class CoreWebView2NavigationStartingEventArgs
    {
        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public bool Cancel  { get; set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public bool IsRedirected { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public bool IsUserInitiated { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public ulong NavigationId { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public CoreWebView2HttpRequestHeaders RequestHeaders { get; internal set; }

        [NotImplemented(new string[] { "NET461", "__SKIA__", "__NETSTD_REFERENCE__" })]
        public string Uri { get; internal set; }

        internal CoreWebView2NavigationStartingEventArgs() {}

    }
}
