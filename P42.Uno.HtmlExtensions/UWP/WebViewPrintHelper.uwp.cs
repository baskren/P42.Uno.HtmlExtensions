using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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

            PrintContent = _webView = new WebView
            {
                Name = "PrintWebView" + (instanceCount++).ToString("D3"),
                DefaultBackgroundColor = Windows.UI.Colors.White,
                Visibility = Visibility.Visible,
                Opacity = 0.0,
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
            await Task.Delay(50);

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

            var kids1 = RootPanel.Children.ToArray();
            System.Diagnostics.Debug.WriteLine("WebViewPrintHelper.");

            await PrintManager.ShowPrintUIAsync();

            var kids2 = RootPanel.Children.ToArray();
            System.Diagnostics.Debug.WriteLine("WebViewPrintHelper.");
        }

        async void _webView_NavigationCompletedA(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _webView.NavigationCompleted -= _webView_NavigationCompletedA;
            _webView.NavigationCompleted += _webView_NavigationCompletedB;
            var contentSize = await _webView.WebViewContentSizeAsync();
            _webView.Width = contentSize.Width;
            _webView.Height = contentSize.Height;

            PrintCanvas.InvalidateMeasure();
            PrintCanvas.UpdateLayout();


            await Task.Delay(50);

            var kids = RootPanel.Children.ToArray();
            System.Diagnostics.Debug.WriteLine("WebViewPrintHelper.");

            _webView.Refresh();
        }

        async void _webView_NavigationCompletedB(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _webView.NavigationCompleted -= _webView_NavigationCompletedB;

            await Task.Delay(50);

            var kids = RootPanel.Children.ToArray();
            System.Diagnostics.Debug.WriteLine("WebViewPrintHelper.");

            NavigationCompleteTCS.TrySetResult(true);
        }
        
        protected override async Task<IEnumerable<UIElement>> GeneratePagesAsync(PrintPageDescription pageDescription)
        {
            var contentSize = await _webView.WebViewContentSizeAsync();


            System.Diagnostics.Debug.WriteLine("WebViewPrintHelper.GenerateWebViewPagesAsync: contentSize[" + contentSize + "]  ImageableRect[" + pageDescription.ImageableRect + "]");

            // how many pages will there be?
            var imagingWidth = Math.Min(pageDescription.ImageableRect.Width, pageDescription.PageSize.Width * (1 - 2 * ApplicationContentMarginLeft));
            var imagingHeight = Math.Min(pageDescription.ImageableRect.Height, pageDescription.PageSize.Height * (1 - 2 * ApplicationContentMarginTop));

            var scaledHeight = imagingWidth * contentSize.Height / contentSize.Width;
            var pageCount = Math.Ceiling(scaledHeight / imagingHeight);

            // create the pages
            var pages = new List<UIElement>();
            for (int i = 0; i < (int)pageCount; i++)
            {
                var panel = GenerateWebViewPanel(pageDescription, i, imagingWidth, imagingHeight);
                pages.Add(panel);
            }
            return pages;
            
        }

        UIElement GenerateWebViewPanel(PrintPageDescription pageDescription, int pageNumber, double imagingWidth, double imagingHeight)
        {
            System.Diagnostics.Debug.WriteLine("WebViewPrintHelper.GenerateWebViewPanel: [" + pageNumber + "] Size:[" + pageDescription.PageSize + "]");

            int sizeCompletedCount = 0;

            var translateY = -imagingHeight * pageNumber;

            var rect = new Windows.UI.Xaml.Shapes.Rectangle
            {
                Tag = new TranslateTransform { Y = translateY },
                Height = imagingHeight,
                Width = imagingWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            
            var panel = new Windows.UI.Xaml.Controls.Grid
            {
                Height = pageDescription.PageSize.Height,
                Width = pageDescription.PageSize.Width,
                Children = { rect },
            };
            
            rect.Loaded += (s, e) =>
            {
                var brush = new WebViewBrush
                {
                    Stretch = Stretch.Uniform,
                };
                brush.SetSource(_webView);
                brush.Redraw();
                brush.Stretch = Stretch.UniformToFill;
                brush.AlignmentY = AlignmentY.Top;
                brush.Transform = rect.Tag as TranslateTransform;
                rect.Fill = brush;
                sizeCompletedCount++;
            };
            rect.Visibility = Visibility.Visible;
            return panel;
        }
    }
}
