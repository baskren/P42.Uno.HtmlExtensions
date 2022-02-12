#if __WASM__ || __MACOS__
#pragma warning disable CS0067, CS0414
#endif

#if XAMARIN || __WASM__ || __SKIA__
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uno.Extensions;
using Uno.UI.Xaml;
using Windows.UI.Xaml;

namespace P42.Uno.HtmlExtensions
{
	public partial class WebViewX : Control
	{
		private const string BlankUrl = "about:blank";
		private static readonly Uri BlankUri = new Uri(BlankUrl);

		private object _internalSource;
		private bool _isLoaded;
		private string _invokeScriptResponse = string.Empty;

		public WebViewX()
		{
			DefaultStyleKey = typeof(WebViewX);
            Loaded += OnLoaded;
		}

#region CanGoBack

        public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
			private set { SetValue(CanGoBackProperty, value); }
		}

		public static DependencyProperty CanGoBackProperty { get; } =
			DependencyProperty.Register("CanGoBack", typeof(bool), typeof(WebViewX), new FrameworkPropertyMetadata(false));

#endregion

#region CanGoForward

		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
			private set { SetValue(CanGoForwardProperty, value); }
		}

		public static DependencyProperty CanGoForwardProperty { get; } =
			DependencyProperty.Register("CanGoForward", typeof(bool), typeof(WebViewX), new FrameworkPropertyMetadata(false));

#endregion

#region Source

		public Uri Source
		{
			get { return (Uri)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public static DependencyProperty SourceProperty { get; } =
			DependencyProperty.Register("Source", typeof(Uri), typeof(WebViewX), new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.Default,
				(s, e) => ((WebViewX)s)?.Navigate((Uri)e.NewValue)));

#endregion

#region DocumentTitle
#if __ANDROID__ || __IOS__ || __MACOS__
		public string DocumentTitle
		{
			get { return (string)GetValue(DocumentTitleProperty); }
			internal set { SetValue(DocumentTitleProperty, value); }
		}

		public static DependencyProperty DocumentTitleProperty { get; } =
			DependencyProperty.Register(nameof(DocumentTitle), typeof(string), typeof(WebViewX), new FrameworkPropertyMetadata(null));
#endif
#endregion

#region IsScrollEnabled
		public bool IsScrollEnabled
		{
			get { return (bool)GetValue(IsScrollEnabledProperty); }
			set { SetValue(IsScrollEnabledProperty, value); }
		}

		public static DependencyProperty IsScrollEnabledProperty { get; } =
			DependencyProperty.Register("IsScrollEnabled", typeof(bool), typeof(WebViewX), new FrameworkPropertyMetadata(true,
				FrameworkPropertyMetadataOptions.Default,
				(s, e) => ((WebViewX)s)?.OnScrollEnabledChangedPartial((bool)e.NewValue)));

		partial void OnScrollEnabledChangedPartial(bool scrollingEnabled);
#endregion

#pragma warning disable 67
		public event TypedEventHandler<WebViewX, WebViewXNavigationStartingEventArgs> NavigationStarting;
		public event TypedEventHandler<WebViewX, WebViewXNavigationCompletedEventArgs> NavigationCompleted;
		public event TypedEventHandler<WebViewX, WebViewXNewWindowRequestedEventArgs> NewWindowRequested;
		public event TypedEventHandler<WebViewX, WebViewUnsupportedUriSchemeIdentifiedEventArgs> UnsupportedUriSchemeIdentified;
#pragma warning restore 67

		//Remove pragma when implemented for Android
#pragma warning disable 0067
		public event WebViewXNavigationFailedEventHandler NavigationFailed;
#pragma warning restore 0067

		public void GoBack()
		{
			GoBackPartial();
		}

		public void GoForward()
		{
			GoForwardPartial();
		}

		public void Navigate(Uri uri)
		{
			this.SetInternalSource(uri ?? BlankUri);
		}

		//
		// Summary:
		//     Loads the specified HTML content as a new document.
		//
		// Parameters:
		//   text:
		//     The HTML content to display in the WebView control.
		public void NavigateToString(string text)
		{
			this.SetInternalSource(text ?? "");
		}

		public void NavigateWithHttpRequestMessage(HttpRequestMessage requestMessage)
		{
			if (requestMessage?.RequestUri == null)
			{
				throw new ArgumentException("Invalid request message. It does not have a RequestUri.");
			}

			SetInternalSource(requestMessage);
		}

		public void Stop()
		{
			StopPartial();
		}

		partial void GoBackPartial();
		partial void GoForwardPartial();
		partial void NavigatePartial(Uri uri);
		partial void NavigateToStringPartial(string text);
		partial void NavigateWithHttpRequestMessagePartial(HttpRequestMessage requestMessage);
		partial void StopPartial();


		private void OnLoaded(object sender, RoutedEventArgs e)
		//private protected override void OnLoaded()
		{
			//base.OnLoaded();

			_isLoaded = true;
			Loaded -= OnLoaded;
		}
		

		private void SetInternalSource(object source)
		{
			_internalSource = source;

			this.UpdateFromInternalSource();
		}

		private void UpdateFromInternalSource()
		{
			var uri = _internalSource as Uri;
			if (uri != null)
			{
				NavigatePartial(uri);
				return;
			}

			var html = _internalSource as string;
			if (html != null)
			{
				NavigateToStringPartial(html);
			}

			var message = _internalSource as HttpRequestMessage;
			if (message != null)
			{
				NavigateWithHttpRequestMessagePartial(message);
			}
		}

		private static string ConcatenateJavascriptArguments(string[] arguments)
		{
			var argument = string.Empty;
			if (arguments != null && arguments.Any())
			{
				argument = string.Join(",", arguments);
			}

			return argument;
		}

		internal void OnUnsupportedUriSchemeIdentified(WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
		{
			UnsupportedUriSchemeIdentified?.Invoke(this, args);
		}

		internal bool GetIsHistoryEntryValid(string url) => url != null && !string.IsNullOrWhiteSpace(url.ToString()) && !url.Equals(BlankUrl, StringComparison.OrdinalIgnoreCase);
	}
}
#endif