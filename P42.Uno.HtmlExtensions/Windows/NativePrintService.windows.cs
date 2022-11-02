#if NET6_0_WINDOWS10_0_19041_0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Web.WebView2.Core;
using Windows.UI.WebUI;

namespace P42.Uno.HtmlExtensions
{
	/// <summary>
	/// Web view extensions service.
	/// </summary>
	class NativePrintService : INativePrintService
	{
        readonly static DependencyProperty JobNameProperty = DependencyProperty.Register("JobName", typeof(string), typeof(ToPdfService), null);
        readonly static DependencyProperty TaskCompletionSourceProperty = DependencyProperty.Register("OnPrintComplete", typeof(TaskCompletionSource<ToFileResult>), typeof(ToPdfService), null);


        /// <summary>
        /// Cans the print.
        /// </summary>
        /// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
        public bool IsAvailable => true;




		public async Task PrintAsync(WebView2 webView, string jobName)
		{
			try
			{
				var result = await webView.CoreWebView2.ExecuteScriptAsync("print()");
            }
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"NativePrintService. : ");
			}
		}

        int instanceCount = 0;


        public async Task PrintAsync(Uri uri, string jobName)
		{
            //var completeTcs = new TaskCompletionSource<bool>();
            var navigateTcv = new TaskCompletionSource<bool>();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var webView = new WebView2()
                {
                    Name = "PrintWebView" + (instanceCount++).ToString("D3"),
                    DefaultBackgroundColor = Microsoft.UI.Colors.White,
                    Visibility = Visibility.Visible,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                webView.DefaultBackgroundColor = Microsoft.UI.Colors.White;
                //webView.Width = PageSize.Default.Width;
                webView.Visibility = Visibility.Visible;
                webView.SetValue(JobNameProperty, jobName);

                var grid = new Grid();
                var content = Platform.RootPanel;

                Platform.RootPage.Content = null;
                Platform.RootPage.Content = grid;

                grid.Children.Add(content);
                grid.Children.Add(webView);

                webView.NavigationCompleted += OnNavigationComplete;

                webView.Tag = navigateTcv;
                webView.Source = uri;


                if (await navigateTcv.Task)
                    await PrintAsync(webView, jobName);

                Platform.RootPage.Content = null;
                grid.Children.Clear();
                Platform.RootPage.Content = content;
            });

        }

        static void OnNavigationComplete(WebView2 webView, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (webView.Tag is TaskCompletionSource<bool> tcs)
            {
				tcs.SetResult(args.IsSuccess);
				return;
            }
			throw new Exception("Cannot locate TaskCompletionSource for WebView.NavigationToString");
        }
    }
}
#endif