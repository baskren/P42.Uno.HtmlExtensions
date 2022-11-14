using System;
using Uno;

namespace P42.Web.WebView2.Core
{
    public class CoreWebView2FrameInfo
    {
        public string Name { get; internal set; }

        public string Source { get; internal set; }

        internal CoreWebView2FrameInfo() {}
    }
}
