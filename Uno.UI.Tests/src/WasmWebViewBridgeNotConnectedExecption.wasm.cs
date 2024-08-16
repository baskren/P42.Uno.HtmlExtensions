using System;



namespace P42.Uno.Xaml.Controls;

public class WasmWebViewBridgeNotConnectedException(string? message = null) : Exception(message ?? DefaultMessage)
{
    public const string DefaultMessage = "The bridge to the WASM native webview is not connected.";
}
