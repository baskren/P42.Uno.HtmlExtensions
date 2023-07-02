using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Uno;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

#pragma warning disable CS0067
namespace P42.Web.WebView2.Core
{
    [NotImplemented]
    public sealed class CoreWebView2Environment : IEquatable<CoreWebView2Environment>
    {
        static uint _instances;
        uint _instance = _instances++;


        public string BrowserVersionString => string.Empty;

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public string UserDataFolder => ApplicationData.Current.TemporaryFolder.ToString();

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public event TypedEventHandler<CoreWebView2Environment, CoreWebView2BrowserProcessExitedEventArgs> BrowserProcessExited;

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public event TypedEventHandler<CoreWebView2Environment, object> ProcessInfosChanged;

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public event TypedEventHandler<CoreWebView2Environment, object> NewBrowserVersionAvailable;


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static IAsyncOperation<CoreWebView2Environment> CreateAsync()
        {
            return Task.Run(() => new CoreWebView2Environment()).AsAsyncOperation();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static IAsyncOperation<CoreWebView2Environment> CreateWithOptionsAsync(string browserExecutableFolder, string userDataFolder, CoreWebView2EnvironmentOptions options)
        {
            return CreateAsync();
        }
        

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static string GetAvailableBrowserVersionString()
        {
            return string.Empty;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static string GetAvailableBrowserVersionString(string browserExecutableFolder)
        {
            return string.Empty;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static int CompareBrowserVersionString(string browserVersionString1, string browserVersionString2)
        {
            return 0;
        }

        internal CoreWebView2Environment() {}

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator ==(CoreWebView2Environment x, CoreWebView2Environment y)
        {
            return x?._instance == y?._instance;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public static bool operator !=(CoreWebView2Environment x, CoreWebView2Environment y)
        {
            return !(x == y);
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public bool Equals(CoreWebView2Environment other)
        {
            return this == other;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override bool Equals(object obj)
        {
            CoreWebView2Environment coreWebView2Environment = obj as CoreWebView2Environment;
            if ((object)coreWebView2Environment != null)
            {
                return this == coreWebView2Environment;
            }

            return false;
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2WebResourceRequest CreateWebResourceRequest(string uri, string Method, IRandomAccessStream postData, string Headers)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<CoreWebView2CompositionController> CreateCoreWebView2CompositionControllerAsync(CoreWebView2ControllerWindowReference ParentWindow)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2PointerInfo CreateCoreWebView2PointerInfo()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2PrintSettings CreatePrintSettings()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IReadOnlyList<CoreWebView2ProcessInfo> GetProcessInfos()
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2ContextMenuItem CreateContextMenuItem(string Label, IRandomAccessStream iconStream, CoreWebView2ContextMenuItemKind Kind)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public IAsyncOperation<CoreWebView2Controller> CreateCoreWebView2ControllerAsync(CoreWebView2ControllerWindowReference ParentWindow)
        {
            throw new NotImplementedException();
        }

        [NotImplemented(new string[] { "__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__" })]
        public CoreWebView2WebResourceResponse CreateWebResourceResponse(IRandomAccessStream Content, int StatusCode, string ReasonPhrase, string Headers)
        {
            throw new NotImplementedException();
        }

    }
}
