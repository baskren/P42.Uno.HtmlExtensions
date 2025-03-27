using Android.Runtime;
using Android.Views;
using System;
using Android.Content;

namespace P42.Uno.HtmlExtensions;

internal static class Display
{
    public static double Scale
    {
        get
        {
            using var displayMetrics = new Android.Util.DisplayMetrics();
            //using var service = Platform.AppContext.GetSystemService(Context.WindowService);
            using var service = global::Uno.UI.ContextHelper.Current.GetSystemService(Context.WindowService);
            using var windowManager = service?.JavaCast<IWindowManager>();
            var display = windowManager?.DefaultDisplay;

            if (Android.OS.Build.VERSION.SdkInt < (Android.OS.BuildVersionCodes)31)
            {
#pragma warning disable CA1422 // Validate platform compatibility
                display?.GetRealMetrics(displayMetrics);
#pragma warning restore CA1422 // Validate platform compatibility
                return displayMetrics?.Density ?? 1;
            }

            return global::Uno.UI.ContextHelper.Current.Resources.DisplayMetrics.Density;
        }
    }
}
