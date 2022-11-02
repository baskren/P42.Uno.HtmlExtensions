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

namespace P42.Uno.HtmlExtensions
{
	/// <summary>
	/// Web view extensions service.
	/// </summary>
	public class NativePrintService : INativePrintService
	{
		/// <summary>
		/// Cans the print.
		/// </summary>
		/// <returns><c>true</c>, if print was caned, <c>false</c> otherwise.</returns>
		public bool IsAvailable()
		{
			return PrintManager.IsSupported() && IntPtr.Size == 8;
		}

		TaskCompletionSource<bool> _printingTCS;
		public async Task PrintAsync(WebView2 webView, string jobName)
		{
			await webView.CoreWebView2.ExecuteScriptAsync("print()");
		}

		public async Task PrintAsync(string html, string jobName)
		{
			var webView = new WebView2();
            webView.NavigationCompleted += OnNavigationComplete;

			var tcs = new TaskCompletionSource<bool>();
			webView.Tag = tcs;
			webView.NavigateToString(html);
			if (await tcs.Task)
				await PrintAsync(webView, jobName);
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