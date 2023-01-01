using System;
using System.Collections.Generic;
using System.Text;
using Windows.Web;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Arguments for WebViewX.NavigationFailed event
    /// </summary>
    public class WebViewXNavigationFailedEventArgs
    {
        /// <summary>
        /// Uri of failed source
        /// </summary>
        public Uri Uri
        {
            get;
            internal set;
        }

        /// <summary>
        /// Error status
        /// </summary>
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
