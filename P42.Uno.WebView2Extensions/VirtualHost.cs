using System.Net;
using System.Text;
using Path = System.IO.Path;

#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

internal class VirtualHost
{
    private const string RootFolder = "WebView2ProjectContentFolders";

    internal static readonly List<string> LocalFolders = [];


    #if __ANDROID__
    internal static Android.App.Activity Activity => ContextHelper.Current as Android.App.Activity ?? throw new Exception("Cannot get Android Activity");
    
    internal static Android.Content.Res.AssetManager Assets => Activity.Assets as Android.Content.Res.AssetManager  ?? throw new Exception("Cannot get Android AssetManager");
    
    internal static string ContentRoot => RootFolder;
    #elif WINDOWS
    internal static string ContentRoot => Package.Current is null 
        ? Path.Combine(AppContext.BaseDirectory, RootFolder)
        : Path.Combine(Package.Current.InstalledLocation.Path, RootFolder);
    #elif BROWSERWASM
    internal static string ContentRoot => string.Empty;
    #else
    internal static string ContentRoot => Path.Combine(AppContext.BaseDirectory, RootFolder);
    #endif


    public static string HostUrl { get; private set; }
    
    static VirtualHost()
    {

#if BROWSERWASM
        HostUrl = $"{WasmWebViewExtensions.GetPageUrl()}";
#else
        var random = new Random();
        var port = random.Next(49152, 65536);
        HostUrl = $"http://localhost:{port}";


        var listener1 = new HttpListener();

        listener1.Prefixes.Add(HostUrl+'/');
        //listener.Prefixes.Add($"https://localhost:{port}"+'/');
        listener1.IgnoreWriteExceptions = true;
        listener1.Start();

        Task.Run(async () =>
        {
            while (true)
            {
                var ctx = await listener1.GetContextAsync();
                _ = Task.Run(() =>
                {
                    try
                    {
                        ctx.Response.StatusCode = HandleRequest(ctx);
                        ctx.Response.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"Virtual Host: CATASTROPHIC SERVER ERROR: [{ex}]");
                    }
                });
            }
        });
#endif
    }
    
    
    
    private static int HandleRequest(HttpListenerContext ctx)
    {
        try
        {
            if (ctx.Request.HttpMethod != "GET")
                return 403;

            if (ctx.Request.Url is not { } uri || string.IsNullOrWhiteSpace(uri.LocalPath))
                return 403;

            var folder = Path.GetDirectoryName(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(folder))
                return 403;

            var extension = Path.GetExtension(uri.LocalPath).ToLower();
            if (extension is ".dll" or ".exe" or ".pdb" or ".uprimarker" or ".aar")
                return 403;

            #if __ANDROID__
            if (extension is ".aar" or ".apk" or ".idsig")
                return 403;
            #elif __IOS__
            if (extension is ".dylib" or ".a")
                return 403;
            #endif


            var relativePath = ctx.Request.Url.AbsolutePath.TrimStart('/');
            var filePath = Path.Combine(ContentRoot, relativePath);

            // prevent directory traversal ("../")
            if (!filePath.StartsWith(ContentRoot, StringComparison.OrdinalIgnoreCase))
                return 403;

            var rootFolder = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
            if (!LocalFolders.Contains(rootFolder))
                return 403;

            if (!WebView2Extensions.ProjectFileExists(filePath) && WebView2Extensions.ProjectFolderExists(filePath))
                filePath += "/index.html";

            if (!WebView2Extensions.ProjectFileExists(filePath))
            {
                WebView2Extensions.CheckForProjectFileRecursively(filePath, out var result);
                return 404;
            }    

            try
            {
                #if __ANDROID__
                using var stream = Assets.Open(filePath);
                var buffer = stream.ReadAllBytesFromStream();
                #else
                var buffer = File.ReadAllBytes(filePath);
                #endif

                ctx.Response.ContentType = MimeMapping.MimeUtility.GetMimeMapping(filePath);
                ctx.Response.ContentLength64 = buffer.Length;
                ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);

                return 200;
            }
            catch (HttpListenerException)
            {
                return 418;
            }
            catch (Exception ex)
            {
                var msg = "Server File Read Error: " + ex;
                var error = Encoding.UTF8.GetBytes(msg);
                ctx.Response.OutputStream.Write(error, 0, error.Length);
                return 500;
            }
        }
        catch (Exception e)
        {
            var msg = "Server Response Prep Error : " + e;
            var error = Encoding.UTF8.GetBytes(msg);
            ctx.Response.OutputStream.Write(error, 0, error.Length);
            return 500;
        }
    }
    


}
