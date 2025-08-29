using System.Net;
using System.Text;
using Path = System.IO.Path;

namespace P42.Uno;

internal class VirtualHost
{
    private static VirtualHost Current = new();

    //TODO: replace collection initiation with new();
    internal static readonly List<string> LocalFolders = ["UnoLib1"];

    internal static string ContentRoot => Path.Combine(AppContext.BaseDirectory, "WebView2ProjectContentFolders");

    public static string HostUrl { get; private set; }
    
    // TODO: Can we add to listener.Prefixes after we call listener.Start()?
    // If so, we can use the port # as a way to apply local folder filtering to each web view?
    
    private VirtualHost()
    {
        Console.WriteLine($"ContentRoot: {ContentRoot}");
        
        var random = new Random();
        var port = random.Next(49152, 65536);
        var listener1 = new HttpListener();
        HostUrl = $"http://localhost:{port}";
        listener1.Prefixes.Add(HostUrl+'/');
        //listener.Prefixes.Add($"https://localhost:{port}"+'/');
        listener1.Start();

        Task.Run(async () =>
        {
            while (true)
            {
                var ctx = await listener1.GetContextAsync();
                _ = Task.Run(() =>
                {
                    ctx.Response.StatusCode = HandleRequest(ctx);
                    ctx.Response.Close();
                });
            }
        });
    }
    
    
    
    private static int HandleRequest(HttpListenerContext ctx)
    {
        if (ctx.Request.HttpMethod != "GET")
            return 403;

        if (ctx.Request.Url is not { } uri || string.IsNullOrWhiteSpace(uri.LocalPath))
            return 403;
        
        Console.WriteLine($"VirtualHost HandleRequest uri:[{uri.AbsoluteUri}]  [{uri.LocalPath}]");

        var folder = Path.GetDirectoryName(uri.LocalPath);
        if (string.IsNullOrWhiteSpace(folder))
            return 403;
        
        var extension = Path.GetExtension(uri.LocalPath).ToLower();
        if (string.IsNullOrWhiteSpace(extension) || extension is ".dll" or ".exe" or ".pdb" or ".uprimarker" or ".aar")
            return 403;

        #if __ANDROID__
        if (extension is ".aar" or ".apk" or ".idsig")
            return 403;
        #elif __DESKTOP__
        #elif __IOS__
        if (extension is ".dylib" or ".a")
            return 403;
        #elif __WASM__
        #elif WINDOWS
        #else
        #endif
        

        var relativePath = ctx.Request.Url.AbsolutePath.TrimStart('/');
        var filePath = Path.Combine(ContentRoot, relativePath);

        // prevent directory traversal ("../")
        if (!filePath.StartsWith(ContentRoot, StringComparison.OrdinalIgnoreCase))
            return 403;

        var rootFolder = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
        if (!LocalFolders.Contains(rootFolder))
            return 403;

        if (!File.Exists(filePath))
            return 404;

        try
        {
            var buffer = File.ReadAllBytes(filePath);
            ctx.Response.ContentType = MimeMapping.MimeUtility.GetMimeMapping(filePath);
            ctx.Response.ContentLength64 = buffer.Length;
            ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
            return 200;
        }
        catch (Exception ex)
        {
            var error = Encoding.UTF8.GetBytes("Server error: " + ex.Message);
            ctx.Response.OutputStream.Write(error, 0, error.Length);
            return 500;
        }
    }
    
}
