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
    
    private static List<string>? _assetFiles;


    private static bool _gettingAssets;
    internal static async Task<List<string>?> GetAssetFilesAsync()
    {
        while (_gettingAssets)
            await Task.Delay(100);
        
        if (_assetFiles != null)
            return _assetFiles;
        
        _gettingAssets = true;
        
        var uriString =   $"{VirtualHost.HostUrl}/{GetBootstrapBase()}/uno-assets.txt";
        using var client = new HttpClient();
        var response = await client.GetAsync(uriString);
        if (!response.IsSuccessStatusCode)
            return null;

        var text = await response.Content.ReadAsStringAsync();
        var lines = text.Split('\n');
        
        /*
        Console.WriteLine("=== uno-assets.txt ===");
        foreach (var line in lines)
            Console.WriteLine($"\t{line}");
        Console.WriteLine("=== uno-assets.txt ===");
        */
        
        _assetFiles = lines.ToList();
        _gettingAssets = false;
        return _assetFiles;
    }

}
#endif
