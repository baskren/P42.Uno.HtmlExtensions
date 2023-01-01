using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Event arguments when a new window has been requested
    /// </summary>
    public class WebViewXNewWindowRequestedEventArgs
    {
        /// <summary>
        /// Has this event been already handled (or are you handling it)?
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }

        /// <summary>
        /// Current uri of document
        /// </summary>
        public Uri Referrer
        {
            get;
            private set;
        }

        /// <summary>
        /// requested uri
        /// </summary>
        public Uri Uri
        {
            get;
            private set;
        }

        internal WebViewXNewWindowRequestedEventArgs(Uri referrer, Uri uri)
        {
            Referrer = referrer;
            Uri = uri;
        }
    }
}
