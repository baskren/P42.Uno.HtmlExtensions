using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
//using Uno.UI;
using Uno.Extensions;
//using Uno.UI.Extensions;
using Microsoft.UI.Xaml;
//using Uno.UI.Web;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Uno.Logging;
using System.IO;
using Uno.UI;
//using Microsoft.Web.WebView2.Core;

namespace P42.Uno.HtmlExtensions
{
	/// <summary>
	/// Alternative WebView for WASM that enables more functionality
	/// </summary>
	public partial class WebViewX
	{
		internal NativeWebView _nativeWebView;
		public readonly string Id;
		

		protected override void OnApplyTemplate()
		{
            if (debug) Console.WriteLine($"WebViewX[{Id}].OnApplyTemplate ENTER");
			base.OnApplyTemplate();

			_nativeWebView = this
				//.FindSubviewsOfType<NativeWebView>()
				//.FirstOrDefault();
				.EnumerateAllChildren().Where(c => c is NativeWebView).FirstOrDefault() as NativeWebView;

			if (_nativeWebView == null)
			{
				var text = $"No view of type {nameof(NativeWebView)} found in children, are you missing one of these types in a template ? ";
				if (debug) Console.WriteLine($"WebViewX[{Id}].OnApplyTemplate ERROR: {text}");
				this.Log().Error(text);
			}
			else
            {
				if (debug) Console.WriteLine($"WebViewX[{Id}].OnApplyTemplate _nativeWebView.Id={_nativeWebView.Id}");
            }
			/*
			var text =
				"<script>" +
				//$"window.name='{ _nativeWebView.Guid}';" +
				"console.log(' window.name: ' + window.name );" +
				$"sessionStorage.setItem('NativeWebViewId','{_nativeWebView.Id}')" +
				"</script>";
			_nativeWebView.SetHtmlAttribute("srcdoc", text);
			*/
			UpdateFromInternalSource();
			if (debug) Console.WriteLine($"WebViewX[{Id}].OnApplyTemplate EXIT");
		}

        internal async Task<string> GetDocumentTitleAsync()
        {
            var result = await this.ExecuteScriptAsync("document.title");
            return result ?? string.Empty;
        }


        /// <summary>
        /// Toggles scrolling
        /// </summary>
        /// <param name="scrollingEnabled"></param>
        partial void OnScrollEnabledChangedPartial(bool scrollingEnabled)
        {
			if (scrollingEnabled)
				_nativeWebView.SetCssStyle(("overflow", "hidden"),("height", "100%"), ("width", "100%"));
			else
				_nativeWebView.SetCssStyle(("overflow", "initial"), ("height", "auto"), ("width", "auto"));
		}

		internal bool OnNavigationStarted(Uri uri)
        {
			var args = new WebViewXNavigationStartingEventArgs(uri);
            if (debug) Console.WriteLine($"WebViewX[{Id}].OnNavigationStarted " + uri);
            NavigationStarting?.Invoke(this, args);
			return args.Cancel;
        }

		internal void OnNavigationCompleted(bool isSuccess, Uri uri, Windows.Web.WebErrorStatus status)
        {
            var args = new WebViewXNavigationCompletedEventArgs(isSuccess, uri, status);
            if (debug) Console.WriteLine($"WebViewX[{Id}].OnNavigationCompleted " + uri);
			NavigationCompleted?.Invoke(this, args);
        }

		internal bool OnNewWindowRequested(Uri referrer, Uri uri)
        {
			var args = new WebViewXNewWindowRequestedEventArgs(referrer, uri);
            if (debug) Console.WriteLine($"WebViewX[{Id}].OnNewWindowRequested " + uri);
            NewWindowRequested?.Invoke(this, args);
			return args.Handled;
		}

		internal void OnNavigationFailed(Uri uri, Windows.Web.WebErrorStatus status)
        {
			var args = new WebViewXNavigationFailedEventArgs(uri, status);
            if (debug) Console.WriteLine($"WebViewX[{Id}].OnNavigationFailed " + uri);
            NavigationFailed?.Invoke(this, args);
        }

		partial void GoBackPartial()
		{
			if (CanGoBack)
				_nativeWebView.GoBack();
		}

		partial void GoForwardPartial()
		{
			if (CanGoForward)
				_nativeWebView.GoForward();
		}

		partial void NavigatePartial(Uri uri)
		{
            if (debug) Console.WriteLine($"WebViewX[{Id}].NavigatePartial(" +uri+")  ENTER");
			if (!VerifyNativeWebViewAvailability())
			{
                if (debug) Console.WriteLine($"WebViewX[{Id}].NavigatePartial(" + uri + ")  EXIT : NativeWebView not available");
                return;
			}

			if (uri.Scheme.Equals("local", StringComparison.OrdinalIgnoreCase))
			{
				// can we access the bundle?
				//var path = $"{NSBundle.MainBundle.BundlePath}/{uri.PathAndQuery}";

				//_nativeWebView.LoadRequest(new NSUrlRequest(new NSUrl(path, false)));
				throw new NotSupportedException("Local loading yet to be supported");
			}
			else
			{
                if (debug) Console.WriteLine($"WebViewX[{Id}].NavigatePartial: Absolute");
				//_nativeWebView.SetHtmlAttribute("src", uri.AbsoluteUri);
				_nativeWebView?.SetInternalSource(uri);
			}

            if (debug) Console.WriteLine($"WebViewX[{Id}].NavigatePartial(" + uri + ")  EXIT");
        }

        partial void NavigateToStringPartial(string text)
        {
            //if (debug) Console.WriteLine($"WebViewX[{Id}].NavigateToStringPartial({text.Substring(0, Math.Min(100, text.Length-1))})");
            if (debug) Console.WriteLine($"WebViewX[{Id}].NavigateToStringPartial({text.Substring(Math.Max(0, text.Length-100),100)})");
            _nativeWebView?.SetInternalSource(text);
		}


        public async Task<string> InvokeScriptAsync(CancellationToken ct, string script, string[] arguments)
			=>  await _nativeWebView.InvokeScriptFunctionAsync(script, arguments);
		

		public async Task<string> InvokeScriptAsync(string script, string[] arguments)
			=> await _nativeWebView.InvokeScriptFunctionAsync(script, arguments);


		//public async Task<string> ExecuteScriptAsync(string script)
		//	=> await _nativeWebView.InvokeScriptAsync(script);

		private bool VerifyNativeWebViewAvailability()
		{
			if (_nativeWebView == null)
			{
				if (_isLoaded)
				{
					var text = "This WebView control instance does not have a native web view child, a Control template may be missing.";
					if (debug) Console.WriteLine($"WebViewX[{Id}].VerifyNativeWebViewAvailability: {text}");
					this.Log().Warn(text);
				}

				return false;
			}

			return true;
		}

		internal void InternalSetCanGoBack(bool value)
			=> CanGoBack = value;

		internal void InternalSetCanGoForward(bool value)
			=> CanGoForward = value;
	}
}
