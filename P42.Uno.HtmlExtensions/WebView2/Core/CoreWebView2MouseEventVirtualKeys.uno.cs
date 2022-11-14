using System;

namespace P42.Web.WebView2.Core
{
    [Flags]
    public enum CoreWebView2MouseEventVirtualKeys : uint
    {
        None = 0x0u,
        LeftButton = 0x1u,
        RightButton = 0x2u,
        Shift = 0x4u,
        Control = 0x8u,
        MiddleButton = 0x10u,
        XButton1 = 0x20u,
        XButton2 = 0x40u
    }
}