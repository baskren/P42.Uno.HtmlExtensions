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
		/// <param name="webView2"></param>
		/// <param name="fileName"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		public async Task<ToFileResult> ToPngAsync(WebView2 webView2, string fileName, int width)
		{
			var taskCompletionSource = new TaskCompletionSource<ToFileResult>();
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				var contentSize = await webView2.WebViewContentSizeAsync();
				System.Diagnostics.Debug.WriteLine("A contentSize=[" + contentSize + "]");
				System.Diagnostics.Debug.WriteLine("A webView.Size=[" + webView2.Width + "," + webView2.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

				webView2.SetValue(BeforeWidthProperty, contentSize.Width);
				webView2.SetValue(BeforeHeightProperty, contentSize.Height);

				webView2.SetValue(PngFileNameProperty, fileName);
				webView2.SetValue(TaskCompletionSourceProperty, taskCompletionSource);
				webView2.SetValue(PngWidthProperty, width);
				webView2.Width = width;

				webView2.NavigationCompleted += NavigationCompleteAAsync;

				NavigationCompleteAAsync(webView2, null);
			});
			return await taskCompletionSource.Task;


        }


        private async void NavigationCompleteAAsync(Microsoft.UI.Xaml.Controls.WebView2 webView2, CoreWebView2NavigationCompletedEventArgs args)
		{

			//if (args.IsSuccess)
			{
				webView2.NavigationCompleted -= NavigationCompleteAAsync;
				var contentSize = await webView2.WebViewContentSizeAsync();
				System.Diagnostics.Debug.WriteLine("A contentSize=[" + contentSize + "]");
				System.Diagnostics.Debug.WriteLine("A webView.Size=[" + webView2.Width + "," + webView2.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

				var width = (int)webView2.GetValue(PngWidthProperty);
				webView2.Width = width;
				webView2.Height = contentSize.Height;

				webView2.NavigationCompleted += NavigationCompleteBAsync;
				webView2.Reload();
			}
			/*
			else
			{
                var onComplete = (Action<string>)webView2.GetValue(TaskCompletionSourceProperty);
                onComplete.Invoke(null);

            }
			*/
        }

		private async void NavigationCompleteBAsync(Microsoft.UI.Xaml.Controls.WebView2 viewView2, CoreWebView2NavigationCompletedEventArgs args)
		{
			viewView2.NavigationCompleted -= NavigationCompleteBAsync;
			viewView2.NavigationCompleted += NavigationCompleteC;

			//IsMainPageChild(webView);

			using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
			{
				System.Diagnostics.Debug.WriteLine("B webView.Size=[" + viewView2.Width + "," + viewView2.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");
				try
				{
					var width = (int)viewView2.GetValue(PngWidthProperty);
					System.Diagnostics.Debug.WriteLine("B width=[" + width + "]");

					var contentSize = await viewView2.WebViewContentSizeAsync();
					System.Diagnostics.Debug.WriteLine("B contentSize=[" + contentSize + "]");
					System.Diagnostics.Debug.WriteLine("B webView.Size=[" + viewView2.Width + "," + viewView2.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

					if (contentSize.Height != viewView2.Height || width != viewView2.Width)
					{
						viewView2.Width = contentSize.Width;
						viewView2.Height = contentSize.Height;
						System.Diagnostics.Debug.WriteLine("B webView.Size=[" + viewView2.Width + "," + viewView2.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");

						viewView2.InvalidateMeasure();
						System.Diagnostics.Debug.WriteLine("B webView.Size=[" + viewView2.Width + "," + viewView2.Height + "] IsOnMainThread=[" + MainThread.IsMainThread + "]");
					}

                    var piclib = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());
                    var fileName = (string)viewView2.GetValue(PngFileNameProperty);
                    var file = await piclib.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await viewView2.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
                    }


					var toFileResult = new ToFileResult(file);
					if (viewView2.GetValue(HtmlStringProperty) is string html &&
						viewView2.GetValue(TaskCompletionSourceProperty) is TaskCompletionSource<ToFileResult> onComplete)
                    {
						onComplete.SetResult(toFileResult);
						return;
					}
					viewView2.Width = (int)viewView2.GetValue(BeforeWidthProperty);
					viewView2.Height = (int)viewView2.GetValue(BeforeHeightProperty);
					viewView2.SetValue(ToFileResultProperty, toFileResult);
					//webView.Reload();
				}
				catch (Exception e)
				{
					var toFileResult = new ToFileResult(e.InnerException?.Message ?? e.Message);
					if (viewView2.GetValue(HtmlStringProperty) is string html &&
						viewView2.GetValue(TaskCompletionSourceProperty) is TaskCompletionSource<ToFileResult> onComplete)
					{
						onComplete.SetResult(toFileResult);
						return;
					}
					viewView2.Width = (int)viewView2.GetValue(BeforeWidthProperty);
					viewView2.Height = (int)viewView2.GetValue(BeforeHeightProperty);
					viewView2.SetValue(ToFileResultProperty, toFileResult);
					//webView.Reload();
				}
                finally
                {
                    viewView2.Reload();
                }
            }
		}

		private void NavigationCompleteC(Microsoft.UI.Xaml.Controls.WebView2 webView2, CoreWebView2NavigationCompletedEventArgs args)
        {
			webView2.NavigationCompleted -= NavigationCompleteC;
			if (webView2.GetValue(TaskCompletionSourceProperty) is TaskCompletionSource<ToFileResult> onComplete)
			{
				try
				{
					if (webView2.GetValue(ToFileResultProperty) is ToFileResult result)
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
				throw new Exception("Failed to get TaskCompletionSource for Windows WebView.ToPngAsync");
			
		}

	}
}
