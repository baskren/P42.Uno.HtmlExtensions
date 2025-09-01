
namespace P42.Uno;

public static class HtmlExtensions
{
    
    /// <summary>
    /// Required initialization method
    /// </summary>
    /// <param name="application"></param>
    /// <param name="window"></param>
    public static void Init(Application application, Window window)
        => WebView2Extensions.Init(application, window);

    /// <summary>
    /// Is HTML printing available on this platform?
    /// </summary>
    /// <returns></returns>
    public static bool CanPrint => WebView2Extensions.CanPrint;
    
    /// <summary>
    /// Print html : may throw printing exceptions
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="html"></param>
    /// <param name="token"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task PrintAsync(UIElement element, string html, CancellationToken token = default)
    {
        if (element.XamlRoot is null)
            throw new ArgumentNullException($"{nameof(element)}.{nameof(element.XamlRoot)}");
        
        await DialogExtensions.AuxiliaryWebViewAsyncProcessor<bool>.Create(
            element.XamlRoot, 
            html, 
            PrintFunction, 
            showWebContent: OperatingSystem.IsWindows(), 
            hideAfterOnContentLoadedTaskComplete: true, 
            cancellationToken:  token);
        return;

        static async Task<bool> PrintFunction(WebView2 webView, CancellationToken localToken)
        {
            await webView.PrintAsync(localToken);
            return true;
        }
        
    }

    /// <summary>
    /// Try printing html : presents errors in a dialog
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="html"></param>
    /// <param name="token"></param>
    /// <returns>true on success</returns>
    public static async Task<bool> TryPrintAsync(UIElement element, string html, CancellationToken token = default)
    {
        try
        {
            await PrintAsync(element, html, token);
            return true;
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(element.XamlRoot!, "HTML Print", ex);
            return false;
        }
    }
    
    /// <summary>
    /// Saves html as PDF : may throw exceptions
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="html"></param>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task SavePdfAsync(UIElement element, string html, PdfOptions? options = null, CancellationToken token = default)
    {
        if (element.XamlRoot is null)
            throw new ArgumentNullException($"{nameof(element)}.{nameof(element.XamlRoot)}");

        var fileName = string.IsNullOrEmpty(options?.Filename)
            ? "document"
            : options.Filename;

        await DialogExtensions.AuxiliaryWebViewAsyncProcessor<bool>.Create(
            element.XamlRoot, 
            html, 
            MakePdfFunction,
            cancellationToken: token);
        return;

        async Task<bool> MakePdfFunction(WebView2 webView, CancellationToken localToken)
        {
            var pdfTask =  webView.TryGeneratePdfAsync(options, localToken);
            await WebView2Extensions.InternalSavePdfAsync(element, pdfTask, fileName, localToken);
            return true;
        }
    }

    public static async Task<bool> TrySavePdfAsync(UIElement element, string html, PdfOptions? options = null,
        CancellationToken token = default)
    {
        try
        {
            await SavePdfAsync(element, html, options, token);
            return true;
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(element.XamlRoot!, "HTML PDF", ex);
            return false;
        }
    }
    
    /// <summary>
    /// Generate PDF
    /// </summary>
    /// <param name="element">UIElement in current page</param>
    /// <param name="html"></param>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <returns>pdf, error</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<(byte[]? pdf, string error)> TryGeneratePdfAsync(
        UIElement element, 
        string html, 
        PdfOptions? options = null, 
        CancellationToken token = default)
    {
        try
        {
            if (element.XamlRoot == null)
                throw new ArgumentNullException($"{nameof(element)}.{nameof(element.XamlRoot)}");

            var result = await DialogExtensions.AuxiliaryWebViewAsyncProcessor<(byte[]? pdf, string error)>.Create(
                element.XamlRoot,
                html,
                MakePdfFunction,
                hideAfterOnContentLoadedTaskComplete: true,
                cancellationToken: token);

            if (result.pdf is null || result.pdf.Length == 0)
                result.error ??= "Empty pdf, unknown failure";

            return result;
        }
        catch (Exception ex)
        {
            return (null, ex.ToString());
        }

        async Task<(byte[]?, string)> MakePdfFunction(WebView2 webView, CancellationToken localToken)
            => await webView.TryGeneratePdfAsync(options, localToken);
        
    }


}
