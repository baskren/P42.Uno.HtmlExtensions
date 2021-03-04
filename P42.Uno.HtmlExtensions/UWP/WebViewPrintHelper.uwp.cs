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
            NavigationCompleteTCS = new TaskCompletionSource<bool>();

            var di = DisplayInformation.GetForCurrentView();

            PrintContent = _webView = new WebView
            {
                Name = "PrintWebView" + (instanceCount++).ToString("D3"),
                DefaultBackgroundColor = Windows.UI.Colors.White,
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Opacity = 1.0,
                Width = di.RawDpiX * 8
            };
            _webView.NavigationCompleted += _webView_NavigationCompletedA;
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

            //await Task.Delay(50);

            if (_sourceWebView!=null)
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

            await Task.Delay(1000);

            await PrintManager.ShowPrintUIAsync();
        }

        private static string[] SetBodyOverFlowHiddenString = new string[] { @"function SetBodyOverFlowHidden() { document.body.style.overflow = 'hidden'; } SetBodyOverFlowHidden();" };

        async void _webView_NavigationCompletedA(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _webView.NavigationCompleted -= _webView_NavigationCompletedA;
            _webView.NavigationCompleted += _webView_NavigationCompletedB;
            var contentSize = await _webView.WebViewContentSizeAsync();
            _webView.Width = contentSize.Width;
            _webView.Height = contentSize.Height;

            _webView.InvalidateMeasure();
            _webView.Refresh();
        }

        async void _webView_NavigationCompletedB(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _webView.NavigationCompleted -= _webView_NavigationCompletedB;

            await Task.Delay(1000);

            NavigationCompleteTCS.TrySetResult(true);
        }


        protected override async Task <IEnumerable<UIElement>> GeneratePagesAsync(PrintPageDescription pageDescription)
        {
            var di = DisplayInformation.GetForCurrentView();
            var displayDpi = di.LogicalDpi;

            var pageCount = Math.Ceiling((96 / displayDpi) * _webView.ActualHeight  / (pageDescription.ImageableRect.Height ));
            
            // create the pages
            var pages = new List<UIElement>();
            for (int i = 0; i < (int)pageCount; i++)
            {
                var panel = GenerateWebViewPanel(pageDescription, i);
                pages.Add(panel);
            }

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
