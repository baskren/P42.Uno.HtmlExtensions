using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Microsoft.Web.WebView2.Core;


#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

public static partial class WebView2Extensions
{
    
    #region Public
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

    //TODO: Make internal
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

    
    /// <summary>
    /// Get the size of a WebView's current content
    /// </summary>
    /// <param name="webView"></param>
    /// <param name="depth"></param>
    /// <param name="callerName"></param>
    /// <returns></returns>
    // ReSharper disable once UnusedParameter.Global
    public static async Task<Windows.Foundation.Size> GetWebViewContentSizeAsync(this WebView2 webView, int depth = 0, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
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
            return await GetWebViewContentSizeAsync(webView, depth + 1, callerName);
        }
        return new Windows.Foundation.Size(contentWidth, contentHeight);
    }
    #endregion
    
    
    #region Internal
    /// <summary>
    /// Used for iOS and Android implementations of PrintAsync()
    /// </summary>
    /// <param name="webView2"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal static object GetNativeWebViewWrapper(this WebView2 webView2)
    {
        if (typeof(CoreWebView2).GetField("_nativeWebView", BindingFlags.Instance | BindingFlags.NonPublic) is not {} nativeWebViewField)
            throw new Exception("Unable to obtain _nativeWebView field information");
        var nativeWebView = nativeWebViewField.GetValue(webView2.CoreWebView2);
        return nativeWebView ?? throw new Exception("Unable to obtain native webview");
    }
    #endregion
    
    
    #region Private
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
    
    #endregion

    
}
