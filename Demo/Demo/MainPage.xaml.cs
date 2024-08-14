using P42.Uno.HtmlExtensions;
using Windows.UI;
using Microsoft.Web.WebView2.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Demo;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{

    #region Constants
    private const string HtmlText = "<html><body><p>This is <i>some</i> <b>HTML</b> text!</p></body></html>";
    private const string AssetPath = "Assets/Html/cbracco.html";
    private readonly Uri _assetUri = new Uri($"http://{AssetPath}");
    private readonly Uri _appAssetUri = new Uri($"ms-appx:///{AssetPath}");
    private const string ExternalUrl = "https://platform.uno";
    private readonly Uri _externalUri = new Uri(ExternalUrl);
    private const string ResourceId = ".Resources.form.html";
    #endregion

    
    #region Properties
    private string Source => (SourceRadioButtons.SelectedItem as string) ?? string.Empty;

    private bool IsAsset => Source.Contains("asset", StringComparison.CurrentCultureIgnoreCase);

    private bool IsExternalUrl => Source.Contains("url", StringComparison.CurrentCultureIgnoreCase);

    private bool IsEmbeddedResource => Source.Contains("resource", StringComparison.CurrentCultureIgnoreCase);

    private bool IsText => Source.Contains("text", StringComparison.CurrentCultureIgnoreCase);
    #endregion

    
    #region VisualElements
    private readonly Grid _spinner = new Grid
    {
        RowDefinitions =
        {
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            new RowDefinition { Height = new GridLength(50) },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
        },
        ColumnDefinitions =
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = new GridLength(50) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
        },
        Background = new SolidColorBrush(Color.FromArgb(64, 128, 128, 128))
    };
    private readonly ProgressRing _ring = new ProgressRing
    {
        BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red),
        Foreground = new SolidColorBrush(Microsoft.UI.Colors.Blue),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch
    };
    #endregion

    
    #region Fields
    private int _printJobCount;
    #endregion

    
    #region Construction / Initialization
    public MainPage()
    {

        this.InitializeComponent();
            
        _printButton.IsEnabled = PrintService.IsAvailable;
        _toPdfButton.IsEnabled = ToPdfService.IsAvailable;
        _webView.SizeChanged += _webView_SizeChanged;
        _fromWebViewToggleSwitch.RegisterPropertyChangedCallback(ToggleSwitch.IsOnProperty, (sender, dp) =>
        {
            if (Source.Contains("url", StringComparison.OrdinalIgnoreCase))
                _fromWebViewToggleSwitch.IsOn = true;
        });
            
        Grid.SetRow(_ring, 1);
        Grid.SetColumn(_ring, 1);

        Grid.SetRowSpan(_spinner, _grid.RowDefinitions.Count);
        Grid.SetColumnSpan(_spinner, _grid.ColumnDefinitions.Count);
        _spinner.Children.Add(_ring);
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await _webView.EnsureCoreWebView2Async();
        _webView.NavigateToString("<html><body><h2>WebView2 and HTML [PDF] and [PRINT] tester</h2><p>How to test:</p><ol><li>Select Source</li><li>Select where output will be generated from</li><li>Select output type: [PDF] or [PRINT]</li></ol></body></html>");
        _webView.CoreWebView2.SetVirtualHostNameToFolderMapping("Assets","Assets", CoreWebView2HostResourceAccessKind.Allow);
    }
    #endregion


    #region Event Handlers
    private void _webView_SizeChanged(object sender, SizeChangedEventArgs e)
        =>  _messageTextBlock.Text = $"SIZE: {e.NewSize}";
    
    private async void OnSourceRadioButtons_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsAsset)
            _webView.Source = _assetUri;
            //_webView.Source = _appAssetUri;
        else if (IsExternalUrl)
        {
            _fromWebViewToggleSwitch.IsOn = true;
            _webView.Source = _externalUri;
        }
        else if (IsEmbeddedResource)
        {
            if (await _webView.NavigateToResourceAsync(ResourceId) is not null)
                _messageTextBlock.Text = $"Could not find EmbeddedResource with ResourceId:[{ResourceId}]";
        }
        else if (IsText)
            _webView.NavigateToString(HtmlText);
    }

    private async void OnToPdfClicked(object sender, RoutedEventArgs e)
    {
        await ShowSpinner();
        await OnToPdfClickedInner();
        await HideSpinner();
    }

    private async void OnPrintClicked(object sender, RoutedEventArgs e)
    {
        await ShowSpinner();
        await OnPrintClickedInner();
        await HideSpinner();
    }
    #endregion


    #region  Helpers
    private async Task ShowSpinner()
    {
#if __IOS__ || __MACCATALYST__
        if (Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Virtual)
            return;
#endif
            
        _ring.IsActive = true;
        _grid.Children.Add(_spinner);
        await Task.Delay(50);
    }

    private async Task HideSpinner()
    {
#if __IOS__ || __MACCATALYST__
        if (Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Virtual)
            return;
#endif

        _grid.Children.Remove(_spinner);
        _ring.IsActive = false;
        await Task.Delay(50);
    }

    private async Task OnPrintClickedInner()
    {
        if (!PrintService.IsAvailable)
        {
            _messageTextBlock.Text = "PRINTING NOT AVAILABLE";
            return;
        }


        if (_fromWebViewToggleSwitch.IsOn)
        {
            try
            {
                await _webView.PrintAsync($"WebView Print Job {_printJobCount++}");
                _messageTextBlock.Text = $"Printed WebView w/ {Source} Source";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainPage. : ");
                _messageTextBlock.Text = $"Exception: {ex.Message} ";
            }

            return;
        }
            
        var sourcePrintJobTitle = $"{Source} Source Print Job {_printJobCount++}";
        if (IsAsset)
            await _appAssetUri.PrintAsync(sourcePrintJobTitle);
        else if (IsExternalUrl)
            await _externalUri.PrintAsync(sourcePrintJobTitle);
        else if (IsEmbeddedResource)
        {
            if (await EmbeddedResourceExtensions.ResourceAsStorageFileAsync(ResourceId) is not StorageFile file)
            {
                _messageTextBlock.Text = $"Could not find EmbeddedResource with ResourceId:[{ResourceId}]";
                return;
            }

            await file.PrintAsync(sourcePrintJobTitle);
        }
        else if (IsText)
            await HtmlText.PrintAsync(sourcePrintJobTitle);
            
        _messageTextBlock.Text = $"Printed {Source} Source";
    }

    private async Task OnToPdfClickedInner()
    {
        if (!ToPdfService.IsAvailable)
        {
            _messageTextBlock.Text = "PDF NOT AVAILABLE";
            return;
        }

        var pdfFileName = $"{Source}.pdf";
        ToFileResult? fileResult = null;
        if (_fromWebViewToggleSwitch.IsOn)
        {
            pdfFileName = $"{Source}_via_WebView.pdf";
            fileResult = await _webView.ToPdfAsync(pdfFileName);
        }
        else if (IsAsset)
            fileResult = await _appAssetUri.ToPdfAsync(pdfFileName);
        else if (IsExternalUrl)
            fileResult = await _externalUri.ToPdfAsync(pdfFileName);
        else if (IsEmbeddedResource)
        {
            if (await EmbeddedResourceExtensions.ResourceAsStorageFileAsync(ResourceId) is not StorageFile file)
            {
                _messageTextBlock.Text = $"Could not find EmbeddedResource with ResourceId:[{ResourceId}]";
                return;
            }

            fileResult = await file.ToPdfAsync(pdfFileName);
        }
        else if (IsText)
            fileResult = await HtmlText.ToPdfAsync(pdfFileName);

        if (fileResult is null)
        {
            _messageTextBlock.Text = "Error: Failed to generate PDF";
            return;
        }

        if (fileResult.IsError)
        {
            _messageTextBlock.Text = "Error: " + fileResult.ErrorMessage;
            return;
        }

        _messageTextBlock.Text = "Success: " + fileResult.StorageFile.Path;
        var shareFile = new Xamarin.Essentials.ShareFile(fileResult.StorageFile.Path)
            { FileName = pdfFileName };
        var shareRequest = new Xamarin.Essentials.ShareFileRequest("P42.Uno.HtmlExtensions PDF", shareFile);
        try
        {
            await Xamarin.Essentials.Share.RequestAsync(shareRequest);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage. : ");
            _messageTextBlock.Text = $"Exception: {ex.Message} ";
        }
            
    }
#endregion
}
