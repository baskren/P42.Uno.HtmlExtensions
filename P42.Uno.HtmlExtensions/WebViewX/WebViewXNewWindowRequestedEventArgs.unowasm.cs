using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    public class WebViewXNewWindowRequestedEventArgs
    {
        public bool Handled
        {
            get;
            set;
        }

        public Uri Referrer
        {
            get;
            private set;
        }

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
