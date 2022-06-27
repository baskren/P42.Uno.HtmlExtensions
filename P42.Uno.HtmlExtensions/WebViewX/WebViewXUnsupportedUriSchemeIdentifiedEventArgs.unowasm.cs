using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Event arguments for unsupported uri scheme
    /// </summary>
    public sealed class WebViewXUnsupportedUriSchemeIdentifiedEventArgs
    {
        /// <summary>
        /// Has this event already been handled (or are you handling it)?
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }

        /// <summary>
        /// Uri that triggered this event
        /// </summary>
        public Uri Uri
        {
            get;
            private set;
        }

        public WebViewXUnsupportedUriSchemeIdentifiedEventArgs(Uri uri)
        {
            Uri = uri;
        }
    }
}
