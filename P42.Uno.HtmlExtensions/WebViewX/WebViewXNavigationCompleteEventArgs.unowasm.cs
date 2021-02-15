using System;
using System.Collections.Generic;
using System.Text;
using Windows.Web;

namespace P42.Uno.HtmlExtensions
{
    public sealed class WebViewXNavigationCompletedEventArgs
    {
        public bool IsSuccess
        {
            get;
            internal set;
        }

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

        internal WebViewXNavigationCompletedEventArgs(bool isSuccess, Uri uri, WebErrorStatus status)
        {
            IsSuccess = isSuccess;
            Uri = uri;
            WebErrorStatus = status;
        }
    }
}
