using System;

namespace P42.Web.WebView2.Core
{
    [Flags]
    public enum CoreWebView2PdfToolbarItems : uint
    {
        None = 0x0u,
        Save = 0x1u,
        Print = 0x2u,
        SaveAs = 0x4u
    }
}
