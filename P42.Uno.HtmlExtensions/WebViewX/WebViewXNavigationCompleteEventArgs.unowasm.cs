using System;
using System.Collections.Generic;
using System.Text;
using Windows.Web;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Argments for WebViewX.NavigationCompleted event
    /// </summary>
    public sealed class WebViewXNavigationCompletedEventArgs
    {
        /// <summary>
        /// True if navigation was successful
        /// </summary>
        public bool IsSuccess
        {
            get;
            internal set;
        }

        /// <summary>
        /// Uri navigated to
        /// </summary>
        public Uri Uri
        {
            get;
            internal set;
        }

        /// <summary>
        /// Error status for navigation
        /// </summary>
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
