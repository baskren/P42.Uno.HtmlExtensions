# P42.Uno.HtmlExtensions

Print and PDF generation extensions for Uno's Windows.UI.Xaml.Controls.WebView2.

## Background

As of UNO Platform version 6.0, WebView2 has matured greatly.  For example, the previous iteration of this library was
about 10x larger due to all the platform specific shenanigans required.  However, the version 6.0 WebView2 is not perfect
as the bulk of the CoreWebView2 API is yet to be supported (and for good reason as it appears to be a beast).

Among the unsupported methods, I most sorely need CoreWebView2's `PrintAsync()` and `PrintToPdfStreamAsync()` methods.
This isn't anything new as I needed them and they were missing in older versions of UNO and I needed them back when I
was using Xamarin.Forms.  Fortunately, makding printing and PDF generation work in Xamarin.Forms taught me a lot - including
how much I hate maintaining platform specific code!  Originally, moving to UNO wasn't any better but, for the most part,
my Xamarin code migrated very well and I was off to the races.

However, with the advent of UNO 6, WebView2 got a complete rework.  And I got to use my favorite keyboard key - the [DELETE] button!
Which , this iteration of printing and pdf generation for UNO platform is a lot lighter.

That being said, not everything about UNO 6's WebView2 implementation is roses.  Specifically, I've found that [navigating to
web content in the application package](https://platform.uno/docs/articles/controls/WebView.html#navigating-to-web-content-in-the-application-package)
to be a bit frustrating.  There a few quirks that can be worked around but the most egregious shortcoming is how this approach doesn't seem
to work with BrowserWasm. Given that I've also learned a few more .net tricks since the last time I touched this code,
I was able to rethink how to do this in a way that is very consistent between platforms AND enabled the display of
Markdown files in the browser, via P42.UNO.MarkdownExtensions.

So, here's the implementation details:

## Getting started


