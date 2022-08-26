namespace P42.Uno.HtmlExtensions
{
    public static partial class Platform
    {
        public static Microsoft.UI.Xaml.Application WinUiApplication { get; private set; }

        public static Microsoft.UI.Xaml.Window WinUiWindow { get; private set; }

        public static void Init(Microsoft.UI.Xaml.Application application, Microsoft.UI.Xaml.Window window)
        {
            WinUiApplication = application;
            WinUiWindow = window;
        }
    }
}
