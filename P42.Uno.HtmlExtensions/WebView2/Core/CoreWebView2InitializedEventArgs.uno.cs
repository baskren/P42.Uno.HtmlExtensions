using System;
using Uno;

namespace P42.UI.Xaml.Controls
{
    public class CoreWebView2InitializedEventArgs
    {
        public Exception Exception { get; internal set; }

        internal CoreWebView2InitializedEventArgs() {}
    }
}
