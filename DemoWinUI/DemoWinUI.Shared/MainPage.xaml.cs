using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using P42.Uno.HtmlExtensions;
using Windows.UI;
using Windows.Storage;
using System.Threading.Tasks;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;


#if __WASM__
using WebView = P42.Uno.HtmlExtensions.WebViewX;
using WebViewNavigationCompletedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationCompletedEventArgs;
#else
using WebView = Microsoft.UI.Xaml.Controls.WebView2;
//using WebViewNavigationCompletedEventArgs = Microsoft.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs;
using WebViewNavigationCompletedEventArgs = Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs;
using WebViewXNavigationStartingEventArgs = Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs;
using WebViewXNavigationFailedEventArgs = Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs;
#endif

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DemoWinUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        WebView _webView = new WebView
        {
        };

        public MainPage()
        {
            this.InitializeComponent();

            //_toPngButton.IsEnabled = false;
            //_toPdfButton.IsEnabled = false;
            _printButton.IsEnabled = P42.Uno.HtmlExtensions.PrintService.IsAvailable;

            Grid.SetRow(_webView, 3);
            _grid.Children.Add(_webView);

            _webView.NavigationCompleted += OnNavigationCompleted;
            //_webView.NavigationFailed += _webView_NavigationFailed;
            _webView.NavigationStarting += _webView_NavigationStarting;

            _webView.SizeChanged += _webView_SizeChanged;


            System.Diagnostics.Debug.WriteLine("MainPage. ASSEMBLY: " + GetType().Assembly.GetName().Name);

        }

        private void _webView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _messageTextBlock.Text = e.NewSize.ToString();
        }

        private void _webView_NavigationStarting(WebView sender, WebViewXNavigationStartingEventArgs args)
        {
        }

        private void _webView_NavigationFailed(object sender, WebViewXNavigationFailedEventArgs e)
        {
        }

        private void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("MainPage.OnNavigationCompleted ");// + args.Uri);
        }

        Button _currentResourceButton;
        StorageFile _currentFile;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _currentResourceButton = _loadHtmlFormButton;
            await LoadResource(_currentResourceButton);
        }

        async void OnLoadTextClicked(object sender, RoutedEventArgs e)
        {
            await LoadResource(sender);
        }

        async Task LoadResource(object sender)
        {
            var resourceId = ".Resources.HtmlForm.html";
            if (sender is Button button && button.Content is string label)
            {
                if (label.ToLower().Contains("html form"))
                    resourceId = ".Resources.HtmlForm.html";
                else if (label.ToLower().Contains("uno html"))
                    resourceId = ".Resources.platform.uno.html";
                else if (label.ToLower().Contains("cbracco"))
                    resourceId = ".Resources.cbracco.html";
            }
            if (await ResourceAsStorageFile(resourceId) is StorageFile file)
            {
                _currentFile = file;
                _webView.Source = new Uri(file.Path); ;
            }
        }

        async void OnToPdfClicked(object sender, RoutedEventArgs e)
        {
            if (!P42.Uno.HtmlExtensions.ToPdfService.IsAvailable)
            {
                _messageTextBlock.Text = "PDF NOT AVAILABLE";
                return;
            }

            await ShowSpinner();

            ToFileResult fileResult = null;
            if (_fromWebViewToggleSwitch.IsOn)
            {
                if (await _webView.ToPdfAsync("WebView.pdf") is ToFileResult fileResult1)
                    fileResult = fileResult1;
            }
            else if (await _currentFile.ToPdfAsync("WebView.pdf") is ToFileResult fileResult2)
                fileResult = fileResult2;

            if (fileResult != null)
            {
                if (!fileResult.IsError)
                {
                    _messageTextBlock.Text = "Success: " + fileResult.StorageFile.Path;
                    var shareFile = new Xamarin.Essentials.ShareFile(fileResult.StorageFile.Path) { FileName = "WebView.pdf" };
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
                else
                {
                    _messageTextBlock.Text = "Error: " + fileResult.ErrorMessage;
                }
            }
            else
            {
                _messageTextBlock.Text = "Error: Failed to generate PDF";
            }

            await HideSpinner();
            
        }

        int count = 0;
        async void OnPrintClicked(object sender, RoutedEventArgs e)
        {
            if (!P42.Uno.HtmlExtensions.PrintService.IsAvailable)
            {
                _messageTextBlock.Text = "PRINTING NOT AVAILABLE";
                return;
            }

            await ShowSpinner();

            if (_fromWebViewToggleSwitch.IsOn)
            {
                try
                {
                    await _webView.PrintAsync("WebView PrintJob" + count++);
                    _messageTextBlock.Text = "PRINTING DONE";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MainPage. : ");
                    _messageTextBlock.Text = $"Exception: {ex.Message} ";
                }
            }
            else
            {
                try
                {
                    await _currentFile.PrintAsync("WebView PrintJob" + count++);
                    _messageTextBlock.Text = "PRINTING DONE";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MainPage. : ");
                    _messageTextBlock.Text = $"Exception: {ex.Message} ";
                }
            }

            await HideSpinner();
        }

        Grid _spinner;
        Microsoft.UI.Xaml.Controls.ProgressRing _ring;
        async Task ShowSpinner()
        {
            if (_spinner is null)
            {
                _ring = new ProgressRing
                {
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red),
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Blue),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                Grid.SetRow(_ring, 1);
                Grid.SetColumn(_ring, 1);

                _spinner = new Grid
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
                    Children =
                {
                    _ring,
                },
                    Background = new SolidColorBrush(Color.FromArgb(64, 128, 128, 128))
                };
            }
            Grid.SetRowSpan(_spinner, _grid.RowDefinitions.Count);
            Grid.SetColumnSpan(_spinner, _grid.ColumnDefinitions.Count);
            _grid.Children.Add(_spinner);
            _ring.IsActive = true;
            await Task.Delay(50);

        }

        async Task HideSpinner()
        {
            _grid.Children.Remove(_spinner);
            _ring.IsActive = false;
            await Task.Delay(50);
        }

        async Task<StorageFile> ResourceAsStorageFile(string resourceId, Assembly asm = null)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException(nameof(resourceId));

            asm = asm ?? GetType().Assembly;

            if (asm.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(resourceId)) is String resourceName)
            {
                using (var inStream = asm.GetManifestResourceStream(resourceName))
                {
                    var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(Path.GetRandomFileName() + ".html");
                    using (var outStream = File.OpenWrite(file.Path))
                    {
                        inStream.CopyTo(outStream);
                    }
                    return file;
                }
            }

            return null;
        }
    }
}
