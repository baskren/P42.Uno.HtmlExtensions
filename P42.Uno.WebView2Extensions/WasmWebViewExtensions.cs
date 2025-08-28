#if BROWSERWASM
using System.Runtime.InteropServices.JavaScript;

namespace P42.Uno;

internal static partial class WasmWebViewExtensions
{

    [JSImport("globalThis.P42_GetPageUrl")]
    internal static partial string GetPageUrl();

    [JSImport("globalThis.P42_BootstrapBase")]
    internal static partial string GetBootstrapBase();

    [JSImport("globalThis.P42_HtmlPrint")]
    internal static partial string HtmlPrint(string html);
    
}
#endif
