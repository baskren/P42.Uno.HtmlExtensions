using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using P42.Uno.HtmlExtensions;
using Windows.UI;
using System.Net;
#if __WASM__
using WebView = P42.Uno.HtmlExtensions.WebViewX;
using WebViewNavigationCompletedEventArgs = P42.Uno.HtmlExtensions.WebViewXNavigationCompletedEventArgs;
#else
using WebView = Windows.UI.Xaml.Controls.WebView;
using WebViewNavigationCompletedEventArgs = Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs;
#endif

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        WebView _webView = new WebView
        {
           // Source = new Uri("https://platform.uno")
           // Source = new Uri("https://slashdot.org")
        };

        public MainPage()
        {
            this.InitializeComponent();

            _toPngButton.IsEnabled = ToPngService.IsAvailable;
            _toPdfButton.IsEnabled = ToPdfService.IsAvailable;

            Grid.SetRow(_webView, 2);
            _grid.Children.Add(_webView);

            _webView.NavigationCompleted += OnNavigationCompleted;
        }

        private void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("MainPage.OnNavigationCompleted " + args.Uri);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _toPngButton.IsEnabled = true;
        }


        async void OnToPngClicked(object sender, RoutedEventArgs e)
        {
            /*
            ShowSpinner();
            if ( await _webView.ToPngAsync("WebView.png") is ToFileResult fileResult)
            {
                if (!fileResult.IsError)
                {
                    _messageTextBlock.Text = "Success: " + fileResult.Result;
                    var shareFile = new Xamarin.Essentials.ShareFile(fileResult.Result, "image/png") { FileName = "WebView.png" };
                    var shareRequest = new Xamarin.Essentials.ShareFileRequest("P42.Uno.HtmlExtensions PNG", shareFile);
                    await Xamarin.Essentials.Share.RequestAsync(shareRequest);
                }
                else
                {
                    _messageTextBlock.Text = "Error: " + fileResult.Result;
                }
            }
            HideSpinner();
            */

            var assembly = GetType().Assembly;
            var name = assembly.GetName().Name;
            var resourceId = name + ".Resources.platform.uno.html";
            System.Diagnostics.Debug.WriteLine("MainPage.OnToPngClicked resourceId = " + resourceId);
            //var resources = assembly.GetManifestResourceNames();
            using (var stream = assembly.GetManifestResourceStream(resourceId))
            {
                using (var reader = new StreamReader(stream))
                {
                    var text = await reader.ReadToEndAsync();
                    _webView.NavigateToString(text);
                }
            }
            //WebClient client = new WebClient();
            //var html = client.DownloadString("https://platform.uno");
            //_webView.NavigateToString(html); 
            

           // _webView.Navigate(new Uri("https://raw.githubusercontent.com/baskren/P42.Uno.HtmlExtensions/webViewBridgeEmbed/Demo/Demo.Shared/Resources/platform.uno.html"));
        }

        async void OnToPdfClicked(object sender, RoutedEventArgs e)
        {
            /*
            ShowSpinner();
            if (await _webView.ToPdfAsync("WebView.pdf") is ToFileResult fileResult)
            {
                if (!fileResult.IsError)
                {
                    _messageTextBlock.Text = "Success: " + fileResult.Result;
                    var shareFile = new Xamarin.Essentials.ShareFile(fileResult.Result, "application/pdf") { FileName = "WebView.pdf" };
                    var shareRequest = new Xamarin.Essentials.ShareFileRequest("P42.Uno.HtmlExtensions PDF", shareFile);
                    await Xamarin.Essentials.Share.RequestAsync(shareRequest);
                }
                else
                {
                    _messageTextBlock.Text = "Error: " + fileResult.Result;
                }
            }
            HideSpinner();
            */
        }

        async void OnPrintClicked(object sender, RoutedEventArgs e)
        {
            /*
            await _webView.PrintAsync("WebView PrintJob");
            System.Diagnostics.Debug.WriteLine("MainPage.OnPrintClicked: DONE");
            */

            await P42.Uno.HtmlExtensions.PrintService.PrintAsync("<Button>button</Button>", "print job name");

            //_webView.Navigate(new Uri("https://platform.uno"));

            /*
            var resources = GetType().Assembly.GetManifestResourceNames();
            using (var stream = GetType().Assembly.GetManifestResourceStream("Demo.Wasm.Resources.slashdot.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var text = await reader.ReadToEndAsync();
                    _webView.NavigateToString(text);
                }
            }


            //_webView.Navigate(new Uri("https://raw.githubusercontent.com/baskren/P42.Uno.HtmlExtensions/webViewBridgeEmbed/Demo/Demo.Shared/Resources/slashdot.html"));
                        */
        }

        Grid _spinner;
        ProgressRing _ring;
        void ShowSpinner()
        {
            if (_spinner is null)
            {
                _ring = new ProgressRing
                {
                    BorderBrush = new SolidColorBrush(Colors.Red),
                    Foreground = new SolidColorBrush(Colors.Blue),
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
        }

        void HideSpinner()
        {
            _grid.Children.Remove(_spinner);
            _ring.IsActive = false;
        }
    }
}
