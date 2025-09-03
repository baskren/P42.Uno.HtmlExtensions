using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;


#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

public static partial class WebView2Extensions
{

    private static Application? _winUiApplication;
    internal static Application WinUiApplication => _winUiApplication ?? throw new NullReferenceException("P42.Uno.WebView2Extensions is not initialized.");

    private static Window? _winUiMainWindow;
    public static Window WinUiMainWindow => _winUiMainWindow ?? throw new NullReferenceException("P42.Uno.WebView2Extensions is not initialized.");
    

    /// <summary>
    /// Required initialization method
    /// </summary>
    /// <param name="application"></param>
    /// <param name="window"></param>
    public static void Init(Application application, Window window)
    {
        _winUiApplication = application;
        _winUiMainWindow = window;
        try
        {
            if (application is IWebView2ProjectContent app)
                app.RegisterProjectProvidedItems();
        }
        catch (Exception ex)
        {
            Log.WriteLine($"P42.Uno.WebView2Extensions.Init RegisterProvidedItems Exception [{ex}]");
        }
    }

}




