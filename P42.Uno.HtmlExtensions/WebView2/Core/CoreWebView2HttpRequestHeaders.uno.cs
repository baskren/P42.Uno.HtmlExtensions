using System;
using System.Collections;
using System.Collections.Generic;
using Uno;
using Windows.Foundation.Metadata;

namespace P42.Web.WebView2.Core
{
    //[NotImplemented]
    public class CoreWebView2HttpRequestHeaders : IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        internal CoreWebView2HttpRequestHeaders() {}

        //[NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public string GetHeader(string name)
        {
            throw new NotImplementedException("The member string CoreWebView2HttpRequestHeaders.GetHeader(string name) is not implemented in Uno.");
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2HttpHeadersCollectionIterator GetHeaders(string name)
        {
            throw new NotImplementedException("The member CoreWebView2HttpHeadersCollectionIterator CoreWebView2HttpRequestHeaders.GetHeaders(string name) is not implemented in Uno.");
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Contains(string name)
        {
            throw new NotImplementedException("The member bool CoreWebView2HttpRequestHeaders.Contains(string name) is not implemented in Uno.");
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void SetHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public void RemoveHeader(string name)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }
}