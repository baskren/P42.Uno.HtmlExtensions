using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

namespace P42.Uno.HtmlExtensions
{
    public class NativeToPdfService : INativeToPdfService
    {
        readonly static DependencyProperty PdfFileNameProperty = DependencyProperty.Register("PdfFileName", typeof(string), typeof(ToPdfService), null);
        readonly static DependencyProperty TaskCompletionSourceProperty = DependencyProperty.Register("OnPdfComplete", typeof(TaskCompletionSource<ToFileResult>), typeof(ToPdfService), null);

        readonly static DependencyProperty PageMarginProperty = DependencyProperty.Register("PageMargin", typeof(PageMargin), typeof(ToPdfService), new PropertyMetadata(PageMargin.Default));
        readonly static DependencyProperty PageSizeProperty = DependencyProperty.Register("PageSize", typeof(PageSize), typeof(ToPdfService), new PropertyMetadata(PageSize.Default));

        public bool IsAvailable 
            => true;

        int instanceCount = 0;

        public async Task<ToFileResult> ToPdfAsync(Uri uri, string fileName, PageSize pageSize, PageMargin margin)
        {
            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var webView = new WebView2()
                {
                    Name = "PdfWebView" + (instanceCount++).ToString("D3"),
                    DefaultBackgroundColor = Microsoft.UI.Colors.White,
                    Visibility = Visibility.Visible,
                };
                
                Platform.RootPanel.Children.Insert(0, webView);

                webView.DefaultBackgroundColor = Microsoft.UI.Colors.White;
                webView.Width = pageSize.Width;
                webView.Visibility = Visibility.Visible;
                webView.SetValue(PdfFileNameProperty, fileName);
                webView.SetValue(TaskCompletionSourceProperty, taskCompletionSource);
                webView.SetValue(PageSizeProperty, pageSize);
                webView.SetValue(PageMarginProperty, margin);

                webView.NavigationCompleted += NavigationCompleteAAsync;
                webView.Source = uri;

                await taskCompletionSource.Task;
                Platform.RootPanel.Children.Remove(webView);
            });
            return await taskCompletionSource.Task;

        }

        private async void NavigationCompleteAAsync(WebView2 webView2, CoreWebView2NavigationCompletedEventArgs args)
        {
            var tcs = webView2.GetValue(TaskCompletionSourceProperty) as TaskCompletionSource<ToFileResult>;
            var fileName = webView2.GetValue(PdfFileNameProperty) as string;
            var pageSize = webView2.GetValue(PageSizeProperty) as PageSize;
            var margin = webView2.GetValue(PageMarginProperty) as PageMargin;

            var toFileResult = await ToPdfAsync(webView2, fileName, pageSize, margin);

            tcs.SetResult(toFileResult);
        }

        public async Task<ToFileResult> ToPdfAsync(Microsoft.UI.Xaml.Controls.WebView2 webView2, string fileName, PageSize pageSize, PageMargin margin)
        {

            var height = pageSize.Height;
            var width = pageSize.Width;
            var orientation = CoreWebView2PrintOrientation.Portrait;
            if (pageSize.Height < pageSize.Width)
            {
                width = pageSize.Height;
                height = pageSize.Width;
                orientation = CoreWebView2PrintOrientation.Landscape;
            }

            var settings = webView2.CoreWebView2.Environment.CreatePrintSettings();
            settings.Orientation = orientation;
            settings.PageHeight = height / 72;
            settings.PageWidth = width / 72;
            settings.MarginLeft = margin.Left / 72;
            settings.MarginTop = margin.Top / 72;
            settings.MarginRight = margin.Right / 72;
            settings.MarginBottom = margin.Bottom / 72;
            settings.ShouldPrintBackgrounds = true;
            settings.ShouldPrintHeaderAndFooter = true;


            //var piclib = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());
            var piclib = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());
            var file = await piclib.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

            try
            {
                if (await webView2.CoreWebView2.PrintToPdfAsync(file.Path, settings))
                    return new ToFileResult(file);

                return new ToFileResult("Failed to generate PDF");
            }
            catch (Exception ex)
            {
                return new ToFileResult($"PDF generate threw exception [{ex.Message}]");
            }
        }

    }
}

