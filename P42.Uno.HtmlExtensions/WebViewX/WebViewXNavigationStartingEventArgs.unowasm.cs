using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    public sealed class WebViewXNavigationStartingEventArgs
    {
        public bool Cancel { get; set; }
        public Uri Uri { get; }

        public WebViewXNavigationStartingEventArgs(Uri uri)
            => Uri = uri;
    }
}
