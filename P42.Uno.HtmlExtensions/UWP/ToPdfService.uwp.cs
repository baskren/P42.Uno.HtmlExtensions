using Microsoft.UI.Xaml;
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

        readonly static DependencyProperty HtmlStringProperty = DependencyProperty.Register("HtmlString", typeof(string), typeof(ToPdfService), null);

        readonly static DependencyProperty BeforeWidthProperty = DependencyProperty.Register("BeforeWidth", typeof(int), typeof(ToPdfService), null);
        readonly static DependencyProperty BeforeHeightProperty = DependencyProperty.Register("BeforeHeight", typeof(int), typeof(ToPdfService), null);

        readonly static DependencyProperty ToFileResultProperty = DependencyProperty.Register("ToFileResult", typeof(string), typeof(ToPdfService), null);

        readonly static DependencyProperty PageMarginProperty = DependencyProperty.Register("PageMargin", typeof(PageMargin), typeof(ToPdfService), new PropertyMetadata(PageMargin.Default));
        readonly static DependencyProperty PageSizeProperty = DependencyProperty.Register("PageSize", typeof(PageSize), typeof(ToPdfService), new PropertyMetadata(PageSize.Default));

        public bool IsAvailable => true;

        int instanceCount = 0;

        public async Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin)
        {
            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var webView = new Microsoft.UI.Xaml.Controls.WebView2()
                {
                    Name = "PrintWebView" + (instanceCount++).ToString("D3"),
                    DefaultBackgroundColor = Microsoft.UI.Colors.White,
                    Visibility = Visibility.Visible,
                };
                //webView.Settings.IsJavaScriptEnabled = true;
                //webView.Settings.IsIndexedDBEnabled = true;

                PrintHelper.RootPanel.Children.Insert(0, webView);

                webView.DefaultBackgroundColor = Microsoft.UI.Colors.White;
                webView.Width = pageSize.Width;
                //webView.Height = PageSize.Default.Height - 72;

                webView.Visibility = Visibility.Visible;

                webView.SetValue(PdfFileNameProperty, fileName);
                webView.SetValue(TaskCompletionSourceProperty, taskCompletionSource);
                webView.SetValue(HtmlStringProperty, html);
                webView.SetValue(PageSizeProperty, pageSize);
                webView.SetValue(PageMarginProperty, margin);
                //webView.Width = width;

                webView.NavigationCompleted += NavigationCompleteAAsync;

                webView.NavigateToString(html);

                await taskCompletionSource.Task;
                PrintHelper.RootPanel.Children.Remove(webView);
            });
            return await taskCompletionSource.Task;

        }

        public async Task<ToFileResult> ToPdfAsync(Microsoft.UI.Xaml.Controls.WebView2 unoWebView, string fileName, PageSize pageSize, PageMargin margin)
        {
            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var contentSize = await unoWebView.WebViewContentSizeAsync();
                System.Diagnostics.Debug.WriteLine("A contentSize=[" + contentSize + "]");
                System.Diagnostics.Debug.WriteLine("A webView.Size=[" + unoWebView.Width + "," + unoWebView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

                unoWebView.SetValue(BeforeWidthProperty, contentSize.Width);
                unoWebView.SetValue(BeforeHeightProperty, contentSize.Height);

                unoWebView.SetValue(PdfFileNameProperty, fileName);
                unoWebView.SetValue(TaskCompletionSourceProperty, taskCompletionSource);
                unoWebView.Width = pageSize.Width - margin.HorizontalThickness;

                unoWebView.NavigationCompleted += NavigationCompleteAAsync;

                NavigationCompleteAAsync(unoWebView, null);
            });
            return await taskCompletionSource.Task;
        }

        private async void NavigationCompleteAAsync(Microsoft.UI.Xaml.Controls.WebView2 webView, CoreWebView2NavigationCompletedEventArgs args)
        {

            if (args.IsSuccess)
            {
                webView.NavigationCompleted -= NavigationCompleteAAsync;
                var contentSize = await webView.WebViewContentSizeAsync();
                System.Diagnostics.Debug.WriteLine("A contentSize=[" + contentSize + "]");
                System.Diagnostics.Debug.WriteLine("A webView.Size=[" + webView.Width + "," + webView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

                var margin = (PageMargin)webView.GetValue(PageMarginProperty);
                var pageSize = (PageSize)webView.GetValue(PageSizeProperty);
                var width = pageSize.Width - margin.HorizontalThickness;
                webView.Width = width;
                webView.Height = contentSize.Height;

                webView.NavigationCompleted += NavigationCompleteBAsync;
                webView.Reload();
            }
            else
            {
                var onComplete = (Action<string>)webView.GetValue(TaskCompletionSourceProperty);
                onComplete.Invoke(null);

            }
        }

        private async void NavigationCompleteBAsync(Microsoft.UI.Xaml.Controls.WebView2 webView, CoreWebView2NavigationCompletedEventArgs args)
        {
            webView.NavigationCompleted -= NavigationCompleteBAsync;
            webView.NavigationCompleted += NavigationCompleteC;

            //IsMainPageChild(webView);

            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                System.Diagnostics.Debug.WriteLine("B webView.Size=[" + webView.Width + "," + webView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");
                try
                {
                    var margin = (PageMargin)webView.GetValue(PageMarginProperty);
                    var pageSize = (PageSize)webView.GetValue(PageSizeProperty);
                    var width = pageSize.Width - margin.HorizontalThickness;
                    System.Diagnostics.Debug.WriteLine("B width=[" + width + "]");

                    var contentSize = await webView.WebViewContentSizeAsync();
                    System.Diagnostics.Debug.WriteLine("B contentSize=[" + contentSize + "]");
                    System.Diagnostics.Debug.WriteLine("B webView.Size=[" + webView.Width + "," + webView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

                    if (contentSize.Height != webView.Height || width != webView.Width)
                    {
                        webView.Width = contentSize.Width;
                        webView.Height = contentSize.Height;
                        System.Diagnostics.Debug.WriteLine("B webView.Size=[" + webView.Width + "," + webView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

                        webView.InvalidateMeasure();
                        System.Diagnostics.Debug.WriteLine("B webView.Size=[" + webView.Width + "," + webView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");
                    }

                    var piclib = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());
                    var fileName = (string)webView.GetValue(PdfFileNameProperty);
                    var orienation = pageSize.Width > pageSize.Height
                        ? CoreWebView2PrintOrientation.Landscape
                        : CoreWebView2PrintOrientation.Portrait;

                    var printSettings = webView.CoreWebView2.Environment.CreatePrintSettings();
                    printSettings.MarginLeft = margin.Left;
                    printSettings.MarginTop = margin.Top;
                    printSettings.MarginRight = margin.Right;
                    printSettings.MarginBottom = margin.Bottom;
                    printSettings.Orientation = orienation;
                    printSettings.PageHeight = pageSize.Height;
                    printSettings.PageWidth = pageSize.Width;

                    await webView.CoreWebView2.PrintToPdfAsync(Path.Combine(piclib.Path, fileName), printSettings);
                    /*
                    var file = await piclib.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        //await webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat., stream);
                    }


                    var toFileResult = new ToFileResult(file);
                    if (webView.GetValue(HtmlStringProperty) is string html &&
                        webView.GetValue(TaskCompletionSourceProperty) is TaskCompletionSource<ToFileResult> onComplete)
                    {
                        onComplete.SetResult(toFileResult);
                        return;
                    }
                    webView.Width = (int)webView.GetValue(BeforeWidthProperty);
                    webView.Height = (int)webView.GetValue(BeforeHeightProperty);
                    webView.SetValue(ToFileResultProperty, toFileResult);
                    //webView.Reload();
                    */
                }
                catch (Exception e)
                {
                    var toFileResult = new ToFileResult(e.InnerException?.Message ?? e.Message);
                    if (webView.GetValue(HtmlStringProperty) is string html &&
                        webView.GetValue(TaskCompletionSourceProperty) is TaskCompletionSource<ToFileResult> onComplete)
                    {
                        onComplete.SetResult(toFileResult);
                        return;
                    }
                    webView.Width = (int)webView.GetValue(BeforeWidthProperty);
                    webView.Height = (int)webView.GetValue(BeforeHeightProperty);
                    webView.SetValue(ToFileResultProperty, toFileResult);
                    //webView.Reload();
                }
                finally
                {
                    webView.Reload();
                }
            }
        }

        private void NavigationCompleteC(Microsoft.UI.Xaml.Controls.WebView2 webView, CoreWebView2NavigationCompletedEventArgs args)
        {
            webView.NavigationCompleted -= NavigationCompleteC;
            if (webView.GetValue(TaskCompletionSourceProperty) is TaskCompletionSource<ToFileResult> onComplete)
            {
                try
                {
                    if (webView.GetValue(ToFileResultProperty) is ToFileResult result)
                    {
                        System.Diagnostics.Debug.WriteLine(GetType() + ".NavigationCompleteC: Complete[" + result.ErrorMessage + "]");
                        onComplete.SetResult(result);
                    }
                    else
                    {
                        onComplete.SetResult(new ToFileResult("unknown error generating PNG."));
                    }
                }
                catch (Exception e)
                {
                    onComplete.SetResult(new ToFileResult(e.Message));
                }
            }
            else
                throw new Exception("Failed to get TaskCompletionSource for UWP WebView.ToPngAsync");

        }

    }
}