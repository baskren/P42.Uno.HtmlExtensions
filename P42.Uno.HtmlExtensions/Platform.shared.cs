using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System.Threading;

namespace P42.Uno.HtmlExtensions;

public static partial class Platform
{
    internal static Microsoft.UI.Xaml.Application WinUiApplication { get; private set; }

    internal static Microsoft.UI.Xaml.Window WinUiWindow { get; private set; }

    internal static Page RootPage
    {
        get
        {
            var rootFrame = WinUiWindow.Content as Frame; //Window.Current.Content as Microsoft.UI.Xaml.Controls.Frame;
            return rootFrame?.Content as Page;
        }
    }

    internal static Panel RootPanel => RootPage?.Content as Panel;

    internal static Thread MainThread { get; private set; }

    internal static DispatcherQueue MainThreadDispatchQueue { get; private set; }


    public static void Init(Microsoft.UI.Xaml.Application application, Microsoft.UI.Xaml.Window window)
    {
        WinUiApplication = application;
        WinUiWindow = window;
        MainThread = Thread.CurrentThread;
        MainThreadDispatchQueue = DispatcherQueue.GetForCurrentThread();

    }
}
