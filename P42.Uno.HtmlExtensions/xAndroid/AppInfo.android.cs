using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.HtmlExtensions;

internal static class AppInfo
{
    public static string Name
    {
        get
        {
            var applicationInfo = global::Uno.UI.ContextHelper.Current.ApplicationInfo;
            var packageManager = global::Uno.UI.ContextHelper.Current.PackageManager;
            return applicationInfo.LoadLabel(packageManager);
        }
    }
}