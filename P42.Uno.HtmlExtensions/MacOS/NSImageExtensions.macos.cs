using AppKit;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions
{
    static class NSImageExtensions
    {
        public static NSData AsPNG(this NSImage image)
        {
            var tiff = image.AsTiff();
            var imageRep = new NSBitmapImageRep(tiff);
            var png = imageRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png);
            return png;
        }

    }
}
