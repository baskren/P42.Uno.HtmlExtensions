using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Uno.UI;
using Uno.Extensions;
using Uno.UI.Extensions;
using Windows.UI.Xaml;
using Uno.UI.Web;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Uno.Logging;
using System.IO;

namespace P42.Uno.HtmlExtensions
{
	public partial class WebViewX
	{
		private NativeWebView _nativeWebView;

		protected override void OnApplyTemplate()
		{
            System.Diagnostics.Debug.WriteLine("WebViewX.OnApplyTemplate");
			base.OnApplyTemplate();

			_nativeWebView = this
				//.FindSubviewsOfType<NativeWebView>()
				//.FirstOrDefault();
				.EnumerateAllChildren().Where(c => c is NativeWebView).FirstOrDefault() as NativeWebView;

			if (_nativeWebView == null)
			{
				this.Log().Error($"No view of type {nameof(NativeWebView)} found in children, are you missing one of these types in a template ? ");
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
		}

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
			NavigationStarting?.Invoke(this, args);
			return args.Cancel;
        }

		internal void OnNavigationCompleted(bool isSuccess, Uri uri, Windows.Web.WebErrorStatus status)
        {
			var args = new WebViewXNavigationCompletedEventArgs(isSuccess, uri, status);
            System.Diagnostics.Debug.WriteLine("WebViewX.OnNavigationCompleted " + uri);
			NavigationCompleted?.Invoke(this, args);
        }

		internal bool OnNewWindowRequested(Uri referrer, Uri uri)
        {
			var args = new WebViewXNewWindowRequestedEventArgs(referrer, uri);
			NewWindowRequested?.Invoke(this, args);
			return args.Handled;
		}

		internal void OnNavigationFailed(Uri uri, Windows.Web.WebErrorStatus status)
        {
			var args = new WebViewXNavigationFailedEventArgs(uri, status);
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
            System.Diagnostics.Debug.WriteLine("WebViewX.NavigatePartial("+uri+")");
			if (!VerifyNativeWebViewAvailability())
			{
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
                System.Diagnostics.Debug.WriteLine("WebViewX.NavigatePartial: Absolute");
				//_nativeWebView.SetHtmlAttribute("src", uri.AbsoluteUri);
				_nativeWebView?.SetInternalSource(uri);
			}
		}

		partial void NavigateToStringPartial(string text)
        {
			_nativeWebView?.SetInternalSource(text);
		}


        //This should be IAsyncOperation<string> instead of Task<string> but we use an extension method to enable the same signature in Win.
        //IAsyncOperation is not available in Xamarin.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<string> InvokeScriptAsync(CancellationToken ct, string script, string[] arguments)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
			throw new NotSupportedException();
		}

		public async Task<string> InvokeScriptAsync(string script, string[] arguments)
		{
			return await _nativeWebView.InvokeScriptAsync(script, arguments);
			//_nativeWebView.PostMessageToTennant
		}


		private bool VerifyNativeWebViewAvailability()
		{
			if (_nativeWebView == null)
			{
				if (_isLoaded)
				{
					this.Log().Warn("This WebView control instance does not have a native web view child, a Control template may be missing.");
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
