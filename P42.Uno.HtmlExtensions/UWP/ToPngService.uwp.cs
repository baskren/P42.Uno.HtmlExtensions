#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace P42.Uno.HtmlExtensions
{
	public class NativeToPngService : INativeToPngService
	{
		readonly static DependencyProperty PngFileNameProperty = DependencyProperty.Register("PngFileName", typeof(string), typeof(ToPngService), null);
		readonly static DependencyProperty TaskCompletionSourceProperty = DependencyProperty.Register("OnPngComplete", typeof(TaskCompletionSource<ToFileResult>), typeof(ToPngService), null);
		//readonly static DependencyProperty WebViewProperty = DependencyProperty.Register("WebView", typeof(Windows.UI.Xaml.Controls.WebView), typeof(ToPngService), null);
		readonly static DependencyProperty HtmlStringProperty = DependencyProperty.Register("HtmlString", typeof(string), typeof(ToPngService), null);
		readonly static DependencyProperty PngWidthProperty = DependencyProperty.Register("PngWidth", typeof(int), typeof(ToPngService), null);

		readonly static DependencyProperty BeforeWidthProperty = DependencyProperty.Register("BeforeWidth", typeof(int), typeof(ToPngService), null);
		readonly static DependencyProperty BeforeHeightProperty = DependencyProperty.Register("BeforeHeight", typeof(int), typeof(ToPngService), null);

		readonly static DependencyProperty ToFileResultProperty = DependencyProperty.Register("ToFileResult", typeof(string), typeof(ToPngService), null);

		public bool IsAvailable => true;


		int instanceCount = 0;


		public async Task<ToFileResult> ToPngAsync(string html, string fileName, int width)
		{
			var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				var webView = new Windows.UI.Xaml.Controls.WebView(WebViewExecutionMode.SameThread)
				{
					Name = "PrintWebView" + (instanceCount++).ToString("D3"),
					DefaultBackgroundColor = Windows.UI.Colors.White,
					Visibility = Visibility.Visible,
				};
				webView.Settings.IsJavaScriptEnabled = true;
				webView.Settings.IsIndexedDBEnabled = true;

				PrintHelper.RootPanel.Children.Insert(0, webView);

				webView.DefaultBackgroundColor = Windows.UI.Colors.White;
				webView.Width = width;
				//webView.Height = PageSize.Default.Height - 72;

				webView.Visibility = Visibility.Visible;

				webView.SetValue(PngFileNameProperty, fileName);
				webView.SetValue(TaskCompletionSourceProperty, taskCompletionSource);
				webView.SetValue(HtmlStringProperty, html);
				webView.SetValue(PngWidthProperty, width);
				//webView.Width = width;

				webView.NavigationCompleted += NavigationCompleteAAsync;
				webView.NavigationFailed += WebView_NavigationFailed;

				webView.NavigateToString(html);

				await taskCompletionSource.Task;
				PrintHelper.RootPanel.Children.Remove(webView);
			});
			return await taskCompletionSource.Task;
		}

		public async Task<ToFileResult> ToPngAsync(WebView unoWebView, string fileName, int width)
		{
			var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				var contentSize = await unoWebView.WebViewContentSizeAsync();
				System.Diagnostics.Debug.WriteLine("A contentSize=[" + contentSize + "]");
				System.Diagnostics.Debug.WriteLine("A webView.Size=[" + unoWebView.Width + "," + unoWebView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

				unoWebView.SetValue(BeforeWidthProperty, contentSize.Width);
				unoWebView.SetValue(BeforeHeightProperty, contentSize.Height);

				unoWebView.SetValue(PngFileNameProperty, fileName);
				unoWebView.SetValue(TaskCompletionSourceProperty, taskCompletionSource);
				unoWebView.SetValue(PngWidthProperty, width);
				unoWebView.Width = width;

				unoWebView.NavigationCompleted += NavigationCompleteAAsync;
				unoWebView.NavigationFailed += WebView_NavigationFailed;

				NavigationCompleteAAsync(unoWebView, null);
			});
			return await taskCompletionSource.Task;
		}


		static void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
		{
			var webView = (Windows.UI.Xaml.Controls.WebView)sender;
			if (webView != null)
			{
				var onComplete = (Action<string>)webView.GetValue(TaskCompletionSourceProperty);
				onComplete.Invoke(null);
			}
		}

		private async void NavigationCompleteAAsync(Windows.UI.Xaml.Controls.WebView webView, WebViewNavigationCompletedEventArgs args)
		{
			webView.NavigationCompleted -= NavigationCompleteAAsync;
			var contentSize = await webView.WebViewContentSizeAsync();
			System.Diagnostics.Debug.WriteLine("A contentSize=[" + contentSize + "]");
			System.Diagnostics.Debug.WriteLine("A webView.Size=[" + webView.Width + "," + webView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

			var width = (int)webView.GetValue(PngWidthProperty);
			webView.Width = width;
			webView.Height = contentSize.Height;

			webView.NavigationCompleted += NavigationCompleteBAsync;
			webView.Refresh();
		}

		private async void NavigationCompleteBAsync(Windows.UI.Xaml.Controls.WebView webView, WebViewNavigationCompletedEventArgs args)
		{
			webView.NavigationCompleted -= NavigationCompleteBAsync;
			webView.NavigationCompleted += NavigationCompleteC;

			//IsMainPageChild(webView);

			using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
			{
				System.Diagnostics.Debug.WriteLine("B webView.Size=[" + webView.Width + "," + webView.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");
				try
				{
					var width = (int)webView.GetValue(PngWidthProperty);
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

					await webView.CapturePreviewToStreamAsync(ms);

					var decoder = await BitmapDecoder.CreateAsync(ms);

					var transform = new BitmapTransform
					{
						ScaledHeight = (uint)Math.Ceiling(webView.Height),
						ScaledWidth = (uint)Math.Ceiling(webView.Width)
					};

					var pixelData = await decoder.GetPixelDataAsync(
						BitmapPixelFormat.Bgra8,
						BitmapAlphaMode.Straight,
						transform,
						ExifOrientationMode.RespectExifOrientation,
						ColorManagementMode.DoNotColorManage);
					var bytes = pixelData.DetachPixelData();


					var piclib = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFolderAsync( Guid.NewGuid().ToString());
					var fileName = (string)webView.GetValue(PngFileNameProperty);
					var file = await piclib.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
					using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
					{
						var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
						encoder.SetPixelData(BitmapPixelFormat.Bgra8,
											BitmapAlphaMode.Ignore,
											(uint)Math.Ceiling(webView.Width),(uint)Math.Ceiling(webView.Height),
											0, 0, bytes);
						await encoder.FlushAsync();
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
					webView.Refresh();
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
					webView.Refresh();
				}
			}
			webView.Refresh();
		}

		private void NavigationCompleteC(Windows.UI.Xaml.Controls.WebView webView, WebViewNavigationCompletedEventArgs args)
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

		/*
        static bool IsMainPageChild(Windows.UI.Xaml.Controls.WebView webView)
		{
			var currentPage = Forms9Patch.PageExtensions.FindCurrentPage(Xamarin.Forms.Application.Current?.MainPage);
			var rootPageRenderer = (Xamarin.Forms.Platform.UWP.PageRenderer)Platform.GetRenderer(currentPage);

			var result = rootPageRenderer.Children.Contains(webView);

			System.Diagnostics.Debug.WriteLine("IsMainPageChild : [" + result + "]  WebView.Parent=[" + webView.Parent + "]");

			return result;
		}
		*/
	}
}
#endif