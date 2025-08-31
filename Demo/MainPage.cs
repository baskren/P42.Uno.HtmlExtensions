using P42.Uno;

namespace Demo;

public sealed partial class MainPage : Page
{
    private readonly WebView2 _webView;

    public MainPage()
    {
        this
            .Background(ThemeResource.Get<Brush>("ApplicationPageBackgroundThemeBrush"))
            .VerticalAlignment(VerticalAlignment.Stretch)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Content(
                new Grid()
                    .SafeArea(SafeArea.InsetMask.All)
                    .VerticalAlignment(VerticalAlignment.Stretch)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .RowDefinitions("50,*,50")
                    .Children
                    (
                        new TextBlock()
                            .Text("Hello World!")
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .VerticalAlignment(VerticalAlignment.Center),
                        new StackPanel()
                            .Grid(row:0)
                            .Orientation(Orientation.Horizontal)
                            .HorizontalAlignment(HorizontalAlignment.Left)
                            .VerticalAlignment(VerticalAlignment.Top)
                            .Children(
                                new Button()
                                    .Name(out var webViewPrintButton)
                                    .Content("WV2 PRINT")
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .IsEnabled(WebView2Extensions.CanPrint),
                                new Button()
                                    .Name(out var webViewPdfButton)
                                    .Content("WV2 PDF")
                                    .VerticalAlignment(VerticalAlignment.Center)
                            ),
                        new WebView2()
                            .Name(out _webView)
                            .Grid(row:1)
                            .DefaultBackgroundColor(Colors.White)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .VerticalAlignment(VerticalAlignment.Stretch),
                        new StackPanel()
                            .Grid(row: 2)
                            .Orientation(Orientation.Horizontal)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Children(
                                new Button()
                                    .Name(out var htmlPrintButton)
                                    .Content("HTML PRINT")
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .IsEnabled(HtmlExtensions.CanPrint),
                                new Button()
                                    .Name(out var htmlPdfButton)
                                    .Content("HTML PDF")
                                    .VerticalAlignment(VerticalAlignment.Center)

                            )



                    )
            );

        webViewPrintButton.Click += OnWebViewPrintButtonClick;
        webViewPdfButton.Click += OnWebViewPdfButtonClick;

        htmlPrintButton.Click += OnHtmlPrintButtonClick;
        htmlPdfButton.Click += OnHtmlPdfButtonClick;

        Loaded += OnLoaded;
    }
    
    private async void OnWebViewPrintButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await _webView.EnsureCoreWebView2Async();
            await _webView.PrintAsync();
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(XamlRoot!, "WebView Print", ex);
        }
    }

    private async void OnWebViewPdfButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var options = new PdfOptions([30, 30, 30, 30],
                Filename: "Document_WebViewPdf",
                Html2canvas: new Html2CanvasOptions(Scale: 2),
                JsPDF: new JsPdfOptions(Unit: PdfUnits.Pt, Format: PdfPageSize.Letter));
            await _webView.SavePdfAsync(options);
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(XamlRoot!, "WebView PDF", ex);
        }

    }

    private async void OnHtmlPrintButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var html = await WebView2Extensions.ReadResourceAsTextAsync("Demo.Resources.Html5TestPage.html", GetType().Assembly);
            await HtmlExtensions.PrintAsync(this, html);
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(XamlRoot!, "Html Print : Demo.Resources.Html5TestPage.html", ex);
        }
    }

    private async void OnHtmlPdfButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var options = new PdfOptions([30, 30, 30, 30],
                Filename: "Document_HtmlPdf",
                Html2canvas: new Html2CanvasOptions(Scale: 2),
                JsPDF: new JsPdfOptions(Unit: PdfUnits.Pt, Format: PdfPageSize.Letter));
            var html = await WebView2Extensions.ReadResourceAsTextAsync("Demo.Resources.Html5TestPage.html", GetType().Assembly);
            await HtmlExtensions.SavePdfAsync(this, html, options);
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(XamlRoot!, "Html PDF : Demo.Resources.Html5TestPage.html", ex);
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _webView.EnsureCoreWebView2Async();
    
            _webView.EnableMarkdownSupport();
                
            //WebView2Extensions.EnableProjectContentFolder("WebContentX");
            //WebView2Extensions.EnableProjectContentFolder("AltWebContent");
            //WebView2Extensions.EnableProjectContentFolder("image");
                
            _webView.NavigateToProjectContentFile("/WebContentX/CltInstall.html");
            //_webView.NavigateToProjectContentFile("/WebContentX/document.md");
                
        }
        catch (Exception ex)
        {
            await DialogExtensions.ShowExceptionDialogAsync(XamlRoot!, "WebView", ex);
        }
    }

    
}
