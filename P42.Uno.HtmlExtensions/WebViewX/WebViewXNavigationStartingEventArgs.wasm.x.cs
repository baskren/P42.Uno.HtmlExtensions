using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Event arguments for WebViewX.NavigationStarting event
    /// </summary>
    public sealed class WebViewXNavigationStartingEventArgs
    {
        /// <summary>
        /// Cancel the navigation?
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Uri destination for navigation
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uri"></param>
        public WebViewXNavigationStartingEventArgs(Uri uri)
            => Uri = uri;
    }
}
