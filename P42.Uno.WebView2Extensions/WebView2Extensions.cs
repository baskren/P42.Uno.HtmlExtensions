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


    /// <summary>
    /// Is printing available on this platform
    /// </summary>
    /// <returns></returns>
    public static bool CanPrint =>
#if __DESKTOP__
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
#else
        true;
#endif

    /// <summary>
    /// Get content of a WebView as HTML
    /// </summary>
    /// <param name="webView"></param>
    /// <returns></returns>
    public static async Task<string> GetHtmlAsync(this WebView2 webView)
        => await webView.ExecuteScriptAsync("document.documentElement.outerHTML;") ?? string.Empty;

    /// <summary>
    /// Wait for document loading to complete
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="token"></param>
    public static async Task WaitForDocumentLoadedAsync(this WebView2 webView2, CancellationToken token = default)
        => await webView2.WaitForVariableValue("document.readyState", "complete", token);

    /// <summary>
    /// Print the contents of a WebView2
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="token"></param>
    public static async Task PrintAsync(this WebView2 webView2, CancellationToken token = default)
    {
            await webView2.WaitForDocumentLoadedAsync(token);
#if __ANDROID__
            var nativeWebViewWrapper = webView2.GetNativeWebViewWrapper();
            var type = nativeWebViewWrapper.GetType();
            if (type.GetProperty
            (
                "WebView", 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            )
            ?.GetValue(nativeWebViewWrapper) is not Android.Webkit.WebView droidWebView)
            throw new Exception("Unable to obtain native webview");

            await droidWebView.PrintAsync(cancellationToken: token);
#elif __IOS__
            var nativeWebViewWrapper = webView2.GetNativeWebViewWrapper();
            if (nativeWebViewWrapper is not WebKit.WKWebView wkWebView)
                throw new Exception("Unable to obtain native webview");

            var result = await wkWebView.PrintAsync();
#else
            await webView2.ExecuteScriptAsync("print();").AsTask(token);

#endif
    }

    /// <summary>
    /// Try printing WebView content : presents errors in a dialog
    /// </summary>
    /// <param name="webView2">unknown result if not on the current page</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<bool> TryPrintAsync(this WebView2 webView2, CancellationToken token = default)
    {
        try
        {
            await PrintAsync(webView2, token);
            return true;
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(webView2.XamlRoot!, "WebView2 Print", ex);
            return false;
        }
        
    }

    /// <summary>
    /// Save the contents of a WebView2 
    /// </summary>
    /// <param name="webView"></param>
    /// <param name="options">per the html2pdf.js library</param>
    /// <param name="token"></param>
    public static async Task SavePdfAsync(this WebView2 webView,  PdfOptions? options = null, CancellationToken token = default)
    {
        if (webView.XamlRoot is null)
            throw new ArgumentNullException($"{nameof(webView)}.{nameof(webView.XamlRoot)}");
        
        var pdfTask = webView.TryGeneratePdfAsync(options, token);

        var fileName = string.IsNullOrEmpty(options?.Filename)
            ? "document"
            : options.Filename;

        await DialogExtensions.BusyDialog.Create(webView.XamlRoot!, "Generating / Saving PDF", MakePdfFunction, cancellationToken: token);
        return;
        
        async Task MakePdfFunction(CancellationToken localToken)
            => await InternalSavePdfAsync(webView, pdfTask, fileName, localToken);
    }

    public static async Task<bool> TrySavePdfAsync(this WebView2 webView, PdfOptions? options = null,
        CancellationToken token = default)
    {
        try
        {
            await SavePdfAsync(webView, options, token);
            return true;
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(webView.XamlRoot!, "WebView2 PDF", ex);
            return false;
        }

    }

    /// <summary>
    /// Generate a (byte[] pdf, string error) from the contents of a WebView2
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="options">html2pdf.js options</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<(byte[]? pdf, string error)> TryGeneratePdfAsync(this WebView2 webView2, PdfOptions? options = null, CancellationToken token = default)
    {
        try
        {
            await webView2.WaitForDocumentLoadedAsync(token);
            await webView2.AssurePdfScriptsAsync(token);

            var result = string.Empty;
            var error = string.Empty;


            var jsonOptions = options == null
                ? ""
                : JsonSerializer.Serialize(options, PdfOptionsSourceGenerationContext.Default.PdfOptions).Trim('"');

            await webView2.ExecuteScriptAsync($"p42_makePdf({jsonOptions})").AsTask(token);
            while (string.IsNullOrWhiteSpace(result) && string.IsNullOrWhiteSpace(error))
            {
                error = await webView2.CoreWebView2.ExecuteScriptAsync("window.p42_makeP42_error").AsTask(token) ?? "";
                //Log.WriteLine($"error:[{error}]");
                error = error.Trim('"').Trim('"');
                if (error == "null")
                    error = string.Empty;
                if (!string.IsNullOrEmpty(error))
                    return (null, error);

                result = await webView2.CoreWebView2.ExecuteScriptAsync("window.p42_makePdf_result").AsTask(token) ?? "";
                result = result.Trim('"').Trim('"');
                //Log.WriteLine($"result: [{result}]");
                if (result == "null")
                    result = string.Empty;
                else if (!string.IsNullOrEmpty(result))
                    Log.WriteLine("bingo");

                await Task.Delay(500, token);
            }

            if (string.IsNullOrWhiteSpace(result))
                return (null, "Empty PDF, unknown failure");

            byte[]? bytes = null;
            try
            {
                bytes = Convert.FromBase64String(result);
            }
            catch (Exception e)
            {
                return (null, $"Base64 conversion exception: \n\n [{e}] \n \n of pdf result: \n\n [{result}]");
            }

            if (bytes is null || bytes.Length == 0)
                return (null, "Empty PDF, unknown failure");

            return (bytes, "");
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.ToString());
            return (null, ex.ToString());
        }
    }

        /// <summary>
    /// Get the size of a WebView's current content
    /// </summary>
    /// <param name="webView"></param>
    /// <param name="depth"></param>
    /// <param name="callerName"></param>
    /// <returns></returns>
    // ReSharper disable once UnusedParameter.Global
    public static async Task<Windows.Foundation.Size> WebViewContentSizeAsync(this WebView2 webView, int depth = 0, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
    {
        ArgumentNullException.ThrowIfNull(webView);

        double contentWidth = -1;
        double contentHeight = -1;

        switch (depth)
        {
            case > 50:
                return new Windows.Foundation.Size(contentWidth, contentHeight);
            case > 0:
                await Task.Delay(100);
                break;
        }

        try
        {
            contentWidth = await webView.TryUpdateIfLarger("document.documentElement.scrollWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.documentElement.scrollHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.documentElement.offsetWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.documentElement.offsetHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.documentElement.getBoundingClientRect().width", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.documentElement.getBoundingClientRect().height", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.documentElement.clientWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.documentElement.clientHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.documentElement.innerWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.documentElement.innerHeight", contentHeight);



            contentWidth = await webView.TryUpdateIfLarger("self.scrollWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("self.scrollHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("self.offsetWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("self.offsetHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("self.getBoundingClientRect().width", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("self.getBoundingClientRect().height", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("self.clientWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("self.clientHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("self.innerWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("self.innerHeight", contentHeight);



            contentWidth = await webView.TryUpdateIfLarger("document.body.scrollWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.body.scrollHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.body.offsetWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.body.offsetHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.body.getBoundingClientRect().width", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.body.getBoundingClientRect().height", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.body.clientWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.body.clientHeight", contentHeight);

            contentWidth = await webView.TryUpdateIfLarger("document.body.innerWidth", contentWidth);
            contentHeight = await webView.TryUpdateIfLarger("document.body.innerHeight", contentHeight);
            
        }
        catch (Exception e)
        {
            var message = $"WebViewExtensions.WebViewContentSizeAsync FAIL: {e.Message}";
            Log.WriteLine(message);
            return await WebViewContentSizeAsync(webView, depth + 1, callerName);
        }
        return new Windows.Foundation.Size(contentWidth, contentHeight);
    }

    
    internal static async Task InternalSavePdfAsync(UIElement element, Task<(byte[]? pdf, string error)> pdfTask, string fileName, CancellationToken token)
    {
        if (element.XamlRoot is null)
            throw new ArgumentNullException($"{nameof(element)}.{nameof(element.XamlRoot)}");

        var fileTask = DialogExtensions.RequestStorageFileAsync( element.XamlRoot, "PDF", fileName, "pdf");
        await Task.WhenAll(fileTask, pdfTask);
        var saveFile = fileTask.Result;

        if (token.IsCancellationRequested || saveFile is null)
            return;

        if (pdfTask.Result.pdf is null || pdfTask.Result.pdf.Length == 0)
        {
            throw new Exception($"PDF Generation Error : {pdfTask.Result.error}");
            //await DialogExtensions.ShowErrorDialogAsync(element.XamlRoot, "PDF Generation Error", pdfTask.Result.error);
            //return;
        }

        if (token.IsCancellationRequested)
            return;

        try
        {
            CachedFileManager.DeferUpdates(saveFile);
#if __DESKTOP__
            await System.IO.File.WriteAllBytesAsync(saveFile.Path, pdfTask.Result.pdf, token);
#else
            await FileIO.WriteBytesAsync(saveFile, pdfTask.Result.pdf);
#endif
            await CachedFileManager.CompleteUpdatesAsync(saveFile);
        }
        catch (Exception e)
        {
            await DialogExtensions.ShowExceptionDialogAsync(element.XamlRoot, "File Save", e);
        }
    }

    /// <summary>
    /// Makes sure that the pdf generation scripts have been loaded
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="token"></param>
    private static async Task AssurePdfScriptsAsync(this WebView2 webView2, CancellationToken token = default)
    {
        await webView2.AssureResourceFunctionLoadedAsync("html2pdf", "P42.Uno.Wv2Ext.Resources.html2pdf.bundle.js", token);
        await webView2.AssureResourceFunctionLoadedAsync("p42_makePdf", "P42.Uno.Wv2Ext.Resources.p42_makePdf.js", token);
    }


    /// <summary>
    /// Used for iOS and Android implementations of PrintAsync()
    /// </summary>
    /// <param name="webView2"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static object GetNativeWebViewWrapper(this WebView2 webView2)
    {
        if (typeof(CoreWebView2).GetField("_nativeWebView", BindingFlags.Instance | BindingFlags.NonPublic) is not {} nativeWebViewField)
            throw new Exception("Unable to obtain _nativeWebView field information");
        var nativeWebView = nativeWebViewField.GetValue(webView2.CoreWebView2);
        return nativeWebView ?? throw new Exception("Unable to obtain native webview");
    }


    /// <summary>
    /// Waits for a JavaScript variable (in the window context) to be set to a particular value
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="variable"></param>
    /// <param name="value"></param>
    /// <param name="token"></param>
    private static async Task WaitForVariableValue(this WebView2 webView2, string variable, string value, CancellationToken token = default)
    {
        await webView2.EnsureCoreWebView2Async().AsTask(token);

        var result = string.Empty;
        while (result != value)
        {
            result = await webView2.CoreWebView2.ExecuteScriptAsync(variable).AsTask(token);
            result = result?.Trim('"');

            await Task.Delay(500, token);
        }
        
    }

    /// <summary>
    /// loads javascripts saved as embedded resources
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="functionName"></param>
    /// <param name="resourceId"></param>
    /// <param name="token"></param>
    /// <exception cref="Exception"></exception>
    private static async Task AssureResourceFunctionLoadedAsync(this WebView2 webView2, string functionName, string resourceId, CancellationToken token = default)
    {
        if (await webView2.IsFunctionLoadedAsync(functionName, token))
            return;

        var script = await ReadResourceAsTextAsync(resourceId, typeof(WebView2Extensions).Assembly).WaitAsync(token);
        await webView2.ExecuteScriptAsync(script).AsTask(token);

        if (await webView2.IsFunctionLoadedAsync(functionName, token))
            return;

        throw new Exception($"Failed to load JavaScript function [{functionName}]");
    }

    /// <summary>
    /// checks if function is available in JavaScript land
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="functionName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<bool> IsFunctionLoadedAsync(this WebView2 webView2, string functionName, CancellationToken token = default)
    {
        var type = await webView2.ExecuteScriptAsync($"typeof {functionName};").AsTask(token);
        type = type?.Trim('"').Trim('"');
        return type?.Contains("function") ?? false;
    }

    // gets text from embedded resource
    public static async Task<string> ReadResourceAsTextAsync(string resourceId, Assembly asm)
    {
        try
        {
            await using var stream = asm.GetManifestResourceStream(resourceId);
            if (stream == null)
                throw new FileNotFoundException("stream is null");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            Log.WriteLine($"[{asm.GetName().Name}] Resources: ");
            foreach (var file in asm.GetManifestResourceNames())
                Log.WriteLine($"\t [{file}]");

            throw new FileNotFoundException($"Resource ({resourceId}) not found in Assembly ({asm.GetName().Name})", ex);
        }
    }
    
    
    
    private record TryResult<T>(bool IsSuccess, T? Value = default);

    /// <summary>
    /// runs a javascript in a WebBView2 and tries to cast it to T
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="script"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static async Task<TryResult<T>> TryExecuteScriptAsync<T>(this WebView2 webView2, string script) where T : IParsable<T>, ISpanParsable<T>, INumber<T>
    {
        try
        {
            var result = await webView2.ExecuteScriptAsync(script);
            if (T.TryParse(result, null, out var v))
                return new TryResult<T>(true, v);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WebViewExtensions.TryExecuteIntScriptAsync {ex.GetType()} : {ex.Message} \n{ex.StackTrace} ");
        }

        return await Task.FromResult(new TryResult<T>(false));
    }

    /// <summary>
    /// Run a script.  Return the larger of source and the result of the script
    /// </summary>
    /// <param name="webView2"></param>
    /// <param name="script"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    private static async Task<double> TryUpdateIfLarger(this WebView2 webView2, string script, double source)
    {
        if (await webView2.TryExecuteScriptAsync<double>(script) is { IsSuccess: true } r1 && r1.Value > source)
            return r1.Value;

        return source;
    }





    #region Project Content Folders


    #region Public
    public static void EnableProjectContentFolder(string projectFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        projectFolder = projectFolder.Trim(DirectorySeparators).Replace('\\', '/');
        
        if (SplitPathSegments(projectFolder) is { Length: > 1 })
            throw new ArgumentException("Only folders in project root are allowed", nameof(projectFolder));

#if !BROWSERWASM
        var fullFolderPath = Path.Combine(VirtualHost.ContentRoot, projectFolder);
        
        if (!ProjectFolderExists(fullFolderPath))
        {
            CheckForProjectFolderRecursively(fullFolderPath, out var result);
            throw new DirectoryNotFoundException($"Project Content Folder Not Found: [{fullFolderPath}] : [{result}]");
        }

        VirtualHost.LocalFolders.AddDistinct(projectFolder);
#endif
    }

    public static void NavigateToProjectContentFile(this WebView2 webView2, string projectContentFilePath)
        => webView2.Source = ProjectContentFileUri(projectContentFilePath);

    public static Uri ProjectContentFileUri(string projectContentFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectContentFilePath);
        projectContentFilePath = projectContentFilePath.TrimStart(DirectorySeparators).Replace('\\', '/');

        var urlAndQuery = projectContentFilePath.Split('?');
        projectContentFilePath = urlAndQuery[0];
        var query = urlAndQuery.Length > 1 
            ? urlAndQuery[1]
            : string.Empty;

        // All paths without an extension are assumed to be directories
        // directories assume we're looking for index.html
        if (projectContentFilePath.EndsWith(Path.DirectorySeparatorChar) || projectContentFilePath.EndsWith(Path.AltDirectorySeparatorChar))
            projectContentFilePath += "index.html";

        if (string.IsNullOrWhiteSpace(Path.GetExtension(projectContentFilePath)))
            projectContentFilePath += Path.DirectorySeparatorChar + "index.html";


        var projectFolder = Path.GetDirectoryName(projectContentFilePath);
        if (string.IsNullOrWhiteSpace(projectFolder))
            throw new ArgumentException("Root project folder is not allowed.");

#if !BROWSERWASM
        var fullFilePath = Path.Combine(VirtualHost.ContentRoot, projectContentFilePath);        
        if (!ProjectFileExists(fullFilePath))
        {
            CheckForProjectFileRecursively(fullFilePath, out var result);
            throw new FileNotFoundException($"Project Content File Not Found: [{fullFilePath}] : [{result}]");
        }

        var rootProjectFolder = SplitPathSegments(projectFolder)[0];
        VirtualHost.LocalFolders.AddDistinct(rootProjectFolder);
#endif

        if (!string.IsNullOrWhiteSpace(query))
            projectContentFilePath += $"?{query}";
        
        var url =  $"{VirtualHost.HostUrl}/{projectContentFilePath}";
        Log.WriteLine($"VirtualHost url: [{url}]");

        return new Uri(url);
    }
#endregion Public


    #region Internal 
    internal static bool CheckForProjectFolderRecursively(string path, out string result)
    {
        result = $"FOUND: {path}";
        // Normalize path separators
        path = Path.GetFullPath(path);

        string current = Path.GetPathRoot(path)!; // e.g. "C:\"
        foreach (var part in path.Substring(current.Length).Split(Path.DirectorySeparatorChar,
                                                                  Path.AltDirectorySeparatorChar,
                                                                  StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, part);

            if (!ProjectFolderExists(current))
            {
                result = $"MISSING: {current}";
                return false;
            }
        }

        return true;
    }

    internal static bool CheckForProjectFileRecursively(string path, out string result)
    {
        result = $"FOUND: {path}";

        var folderPath = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(folderPath) && !CheckForProjectFolderRecursively(folderPath, out result))
            return false;

        if (!ProjectFileExists(path))
        {
            var files = Directory.GetFiles(folderPath ?? "");
            foreach (var file in files) 
                Log.WriteLine($"\t\t{folderPath}/{file}");

            result = $"MISSING: {path}";
            return false;
        }
        return true;
    }

#if BROWSERWASM
    private static readonly HttpClient HttpClient = new HttpClient();
#endif

    internal static bool ProjectFolderExists(string fullFolderPath)
    {
#if __ANDROID__
        try
        {
            var files = VirtualHost.Assets.List(fullFolderPath);
            if (files is null || files.Length == 0)
                return false;
            return true;
        }
        catch 
        { 
            return false; 
        }
#else
        return Directory.Exists(fullFolderPath);
#endif
    }

    internal static bool ProjectFileExists(string fullFilePath)
    {
#if __ANDROID__
        var folderPath = Path.GetDirectoryName(fullFilePath);
        try
        {
            using var stream = VirtualHost.Assets.Open(fullFilePath);
            return stream != null;
            /*
            var files = VirtualHost.Assets.List(folderPath ?? "");
            if (files is null || files.Length == 0)
                return false;
            var fileName = Path.GetFileName(fullFilePath);
            foreach (var file in files)
                if (file == fileName)
                    return true;
            return false;
            */
            
        }
        catch
        {
            return false;
        }
#elif BROWSERWASM
        return false;
#else
        return File.Exists(fullFilePath);
#endif
    }
    #endregion Internal


    #region Private

    private static readonly char[] DirectorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    private static string[] SplitPathSegments(string? path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? []
            : path.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
    }


    #endregion Private


#endregion Project Content Folders
}




