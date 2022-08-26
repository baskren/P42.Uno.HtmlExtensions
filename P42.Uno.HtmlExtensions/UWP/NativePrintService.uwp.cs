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
			if (IntPtr.Size != 8)
				throw new Exception("Printing not available in 32 bit applications.  Blame Microsoft's UWP Team as they don't seem to want to fix a memory leak that keeps printing from working reliably.");

			if (_printingTCS != null)
				return;

			_printingTCS = new TaskCompletionSource<bool>();
			MainThread.BeginInvokeOnMainThread(async () =>
			{

				if (string.IsNullOrWhiteSpace(jobName))
					jobName = Windows.ApplicationModel.Package.Current.DisplayName;
				WebViewPrintHelper printHelper = null;
				if (webView.Source is Uri Uri && !string.IsNullOrWhiteSpace(Uri.AbsolutePath))
                {
					printHelper = new WebViewPrintHelper(Uri, jobName);
                }
				else if (webView is Microsoft.UI.Xaml.Controls.WebView2 nativeWebView)
				{
					printHelper = new WebViewPrintHelper(webView, jobName);
				}
				if (printHelper != null)
				{
					await Task.Delay(50);
					await printHelper.InitAsync();
				}
				_printingTCS?.TrySetResult(true);
			});
			await _printingTCS.Task;

			_printingTCS = null;
			
			/*
			//await webView.InvokeScriptAsync("window.print()", new string[] { });
			try
			{
				//var result = await webView.InvokeScriptAsync("eval", new string[] { "(2 + 2).toString()" });
				//var result = await webView.InvokeScriptAsync("eval", new string[] { "(2 + 2).toString();'pizza';" });  // returns pizza

				var result = await webView.InvokeScriptAsync("eval", new string[] { "window.print();'pizza';" });  // returns 'pizza', but doesn't print.
				//var result = await webView.InvokeScriptAsync("Function", new string[] { "\"use strict\"; return ('pizza')" });

				System.Diagnostics.Debug.WriteLine("NativePrintService.PrintAsync: " + result);

				//result = await webView.InvokeScriptAsync("eval")
			}
			catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("NativePrintService.EXCEPTION: " + e);
            }
			*/
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