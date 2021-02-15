using System;
using System.Collections.Generic;
using System.Text;
using Windows.Web;

namespace P42.Uno.HtmlExtensions
{
    public class WebViewXNavigationFailedEventArgs
    {
        public Uri Uri
        {
            get;
            internal set;
        }

        public WebErrorStatus WebErrorStatus
        {
            get;
            internal set;
        }

        internal WebViewXNavigationFailedEventArgs(Uri uri, WebErrorStatus status)
        {
            Uri = uri;
            WebErrorStatus = status;
        }
    }
}
