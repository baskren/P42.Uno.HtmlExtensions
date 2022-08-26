#if NET6_0_WINDOWS10_0_19041_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Web.WebView2.Core;

namespace P42.Uno.HtmlExtensions
{
	public class NativeToPngService : INativeToPngService
	{
		readonly static DependencyProperty PngFileNameProperty = DependencyProperty.Register("PngFileName", typeof(string), typeof(ToPngService), null);
		readonly static DependencyProperty TaskCompletionSourceProperty = DependencyProperty.Register("OnPngComplete", typeof(TaskCompletionSource<ToFileResult>), typeof(ToPngService), null);

		readonly static DependencyProperty HtmlStringProperty = DependencyProperty.Register("HtmlString", typeof(string), typeof(ToPngService), null);
		readonly static DependencyProperty PngWidthProperty = DependencyProperty.Register("PngWidth", typeof(int), typeof(ToPngService), null);

		readonly static DependencyProperty BeforeWidthProperty = DependencyProperty.Register("BeforeWidth", typeof(int), typeof(ToPngService), null);
		readonly static DependencyProperty BeforeHeightProperty = DependencyProperty.Register("BeforeHeight", typeof(int), typeof(ToPngService), null);

		readonly static DependencyProperty ToFileResultProperty = DependencyProperty.Register("ToFileResult", typeof(string), typeof(ToPngService), null);

		/// <summary>
		/// Is PNG conversion available
		/// </summary>
		public bool IsAvailable => true;


		int instanceCount = 0;

		/// <summary>
		/// Convert HTML to PNG
		/// </summary>
		/// <param name="html"></param>
		/// <param name="fileName"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		public async Task<ToFileResult> ToPngAsync(string html, string fileName, int width)
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
				webView.Width = width;
				//webView.Height = PageSize.Default.Height - 72;

				webView.Visibility = Visibility.Visible;

				webView.SetValue(PngFileNameProperty, fileName);
				webView.SetValue(TaskCompletionSourceProperty, taskCompletionSource);
				webView.SetValue(HtmlStringProperty, html);
				webView.SetValue(PngWidthProperty, width);
				//webView.Width = width;

				webView.NavigationCompleted += NavigationCompleteAAsync;

				webView.NavigateToString(html);

				await taskCompletionSource.Task;
				PrintHelper.RootPanel.Children.Remove(webView);
			});
			return await taskCompletionSource.Task;
		}

		/// <summary>
		/// Convert contents of WebView to PNG
		/// </summary>
		/// <param name="unoWebView"></param>
		/// <param name="fileName"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		public async Task<ToFileResult> ToPngAsync(WebView2 unoWebView, string fileName, int width)
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

				var width = (int)webView.GetValue(PngWidthProperty);
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

                    var piclib = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());
                    var fileName = (string)webView.GetValue(PngFileNameProperty);
                    var file = await piclib.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
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
#endif