using System.Text.Json;

namespace P42.Uno;

public static partial class WebView2Extensions
{
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
                error = error.Trim('"').Trim('"');
                if (error == "null")
                    error = string.Empty;
                if (!string.IsNullOrEmpty(error))
                    return (null, error);

                result = await webView2.CoreWebView2.ExecuteScriptAsync("window.p42_makePdf_result").AsTask(token) ?? "";
                result = result.Trim('"').Trim('"');
                if (result == "null")
                    result = string.Empty;

                await Task.Delay(500, token);
            }

            if (string.IsNullOrWhiteSpace(result))
                return (null, "Empty PDF, unknown failure");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(result);
            }
            catch (Exception e)
            {
                return (null, $"Base64 conversion exception: \n\n [{e}] \n \n of pdf result: \n\n [{result}]");
            }

            return bytes.Length == 0 
                ? (null, "Empty PDF, unknown failure") 
                : (bytes, "");
        }
        catch (Exception ex)
        {
            return (null, ex.ToString());
        }
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


}
