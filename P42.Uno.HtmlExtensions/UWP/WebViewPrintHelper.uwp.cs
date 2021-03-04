using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Printing;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Printing;

namespace P42.Uno.HtmlExtensions
{
    class WebViewPrintHelper : PrintHelper
    {
        WebView _webView;
        WebView _sourceWebView;
        string Html;
        string BaseUrl;
        Uri Uri;

        const string LocalScheme = "ms-appx-web:///";
        const string BaseInsertionScript = @"
var head = document.getElementsByTagName('head')[0];
var bases = head.getElementsByTagName('base');
if(bases.length == 0){
    head.innerHTML = 'baseTag' + head.innerHTML;
}";

        internal WebViewPrintHelper(WebView webView, string jobName) : base(jobName)
        {
            _sourceWebView = webView;
        }

        internal WebViewPrintHelper(string html, string baseUri, string jobName): base(jobName)
        {
            Html = html;
            BaseUrl = baseUri;
            if (string.IsNullOrEmpty(BaseUrl))
                BaseUrl = LocalScheme;
        }

        internal WebViewPrintHelper(Uri uri, string jobName) : base(jobName)
        {
            Uri = uri;
        }

        int instanceCount = 0;

        TaskCompletionSource<bool> NavigationCompleteTCS;
        public override async Task InitAsync()
        {
            await base.InitAsync();
            PrintSpinner = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(100,0,0,0)),
                Children =
                        {
                            new ProgressRing
                            {
                                IsActive = true,
                                Foreground = new SolidColorBrush(Colors.Blue),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
            };

            await PrintManager.ShowPrintUIAsync();
        }

        async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _webView.NavigationCompleted -= WebView_NavigationCompleted;
            GC.Collect();
            await Task.Delay(1000);
            NavigationCompleteTCS.TrySetResult(true);
        }


        protected override async Task <IEnumerable<UIElement>> GeneratePagesAsync(PrintPageDescription pageDescription)
        {
            // Clear out any old webviews and add a new one (need a fresh start)
            if (PrintContent != null && RootPanel.Children.Contains(PrintContent))
            {
                RootPanel.Children.Remove(PrintSpinner);
                RootPanel.Children.Remove(PrintContent);
            }

            PrintContent = _webView = new WebView
            {
                Name = "PrintWebView" + (instanceCount++).ToString("D3"),
                DefaultBackgroundColor = Windows.UI.Colors.White,
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Opacity = 0.0,
                Width = pageDescription.ImageableRect.Width
            };

            if (RootPanel is Grid grid)
            {
                if ((grid.RowDefinitions?.Count ?? 1) > 1)
                {
                    Grid.SetRowSpan(_webView, grid.RowDefinitions.Count);
                    Grid.SetRowSpan(PrintSpinner, grid.RowDefinitions.Count);
                }
                else if ((grid.ColumnDefinitions?.Count ?? 1) > 1)
                {
                    Grid.SetColumnSpan(_webView, grid.ColumnDefinitions.Count);
                    Grid.SetColumnSpan(PrintSpinner, grid.ColumnDefinitions.Count);
                }
            }
            RootPanel.Children.Add(PrintContent);
            RootPanel.Children.Add(PrintSpinner);

            // Initial loading of content in order to get content size
            NavigationCompleteTCS = new TaskCompletionSource<bool>();
            _webView.NavigationCompleted += WebView_NavigationCompleted;
            if (_sourceWebView != null)
            {
                Html = await _sourceWebView.GetHtml();
                _webView.NavigateToString(Html);
            }
            else if (Uri is Uri uri && !string.IsNullOrWhiteSpace(uri.AbsolutePath))
            {
                if (!uri.IsAbsoluteUri)
                    uri = new Uri(LocalScheme + Uri, UriKind.RelativeOrAbsolute);
                _webView.Source = uri;
            }
            await NavigationCompleteTCS.Task;
            
            // required resize and refresh in order to have content available for use
            var contentSize = await _webView.WebViewContentSizeAsync();
            _webView.Height = contentSize.Height;
            NavigationCompleteTCS = new TaskCompletionSource<bool>();
            _webView.NavigationCompleted += WebView_NavigationCompleted;
            _webView.InvalidateMeasure();
            _webView.Refresh();
            await NavigationCompleteTCS.Task;
            
            /*
            System.Diagnostics.Debug.WriteLine($"WebViewPrintHelper. displayDpi=[{displayDpi}]");
            System.Diagnostics.Debug.WriteLine($"WebViewPrintHelper. pageDescription.ImageableRect=[{pageDescription.ImageableRect}]");
            System.Diagnostics.Debug.WriteLine("GeneratePagesAsync. webView Actual=[" + _webView.ActualWidth + ", " + _webView.ActualHeight + "]");
            System.Diagnostics.Debug.WriteLine($"GeneratePagesAsync. webView Desired=[{_webView.DesiredSize}]");
            */
            var pageCount = Math.Ceiling(contentSize.Height / (pageDescription.ImageableRect.Height));

            // create the pages
            var pages = new List<UIElement>();
            for (int i = 0; i < (int)pageCount; i++)
            {
                var panel = GenerateWebViewPanel(pageDescription, i);
                PrintCanvas.Children.Add(panel);
                printPreviewPages.Add(panel);
                pages.Add(panel);
            }

            GC.Collect();
            return pages;
        }

        UIElement GenerateWebViewPanel(PrintPageDescription pageDescription, int pageNumber)
        {
            var brush = new WebViewBrush
            {
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Transform = new TranslateTransform { Y = -pageDescription.ImageableRect.Height * pageNumber },
                SourceName = _webView.Name,
            };
            brush.Redraw();

            var rect = new Windows.UI.Xaml.Shapes.Rectangle
            {
                Height = pageDescription.ImageableRect.Height,
                Width = pageDescription.ImageableRect.Width,
                Visibility = Visibility.Visible,
                Margin = new Thickness(pageDescription.ImageableRect.Left, pageDescription.ImageableRect.Top, 0, 0)
            };

            rect.Loaded += Rect_Loaded;
            void Rect_Loaded(object sender, RoutedEventArgs e)
            {
                rect.Fill = brush;
                rect.Loaded -= Rect_Loaded;
            }

            var panel = new Windows.UI.Xaml.Controls.Grid
            {
                Height = pageDescription.PageSize.Height,
                Width = pageDescription.PageSize.Width,
                Children = { rect },
            };


            return panel;
        }

    }
}
