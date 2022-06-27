#pragma warning disable CS0067, CS0414

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

		/// <summary>
		/// Constructor
		/// </summary>
		public WebViewX()
		{
			DefaultStyleKey = typeof(WebViewX);
            Loaded += OnLoaded;
		}

		#region CanGoBack
		/// <summary>
		/// True if WebView can navigate back
		/// </summary>
        public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
			private set { SetValue(CanGoBackProperty, value); }
		}

		public static DependencyProperty CanGoBackProperty { get; } =
			DependencyProperty.Register("CanGoBack", typeof(bool), typeof(WebViewX), new FrameworkPropertyMetadata(false));

		#endregion

		#region CanGoForward
		/// <summary>
		/// True if WebView can navigate forward
		/// </summary>
		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
			private set { SetValue(CanGoForwardProperty, value); }
		}

		public static DependencyProperty CanGoForwardProperty { get; } =
			DependencyProperty.Register("CanGoForward", typeof(bool), typeof(WebViewX), new FrameworkPropertyMetadata(false));

		#endregion

		#region Source
		/// <summary>
		/// Source for WebView content
		/// </summary>
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

		#region IsScrollEnabled
		/// <summary>
		/// True if scrolling is enabled for webview
		/// </summary>
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
		/// <summary>
		/// Fired when navigation is starting
		/// </summary>
		public event TypedEventHandler<WebViewX, WebViewXNavigationStartingEventArgs> NavigationStarting;
		/// <summary>
		/// Fired when navigation has completed
		/// </summary>
		public event TypedEventHandler<WebViewX, WebViewXNavigationCompletedEventArgs> NavigationCompleted;
		/// <summary>
		/// Fired when a new when has been requested
		/// </summary>
		public event TypedEventHandler<WebViewX, WebViewXNewWindowRequestedEventArgs> NewWindowRequested;
		/// <summary>
		/// Fired when an unsupported Uri scheme has been requested
		/// </summary>
		public event TypedEventHandler<WebViewX, WebViewUnsupportedUriSchemeIdentifiedEventArgs> UnsupportedUriSchemeIdentified;
#pragma warning restore 67

		//Remove pragma when implemented for Android
#pragma warning disable 0067
		/// <summary>
		/// Fired when navigation has failed
		/// </summary>
		public event WebViewXNavigationFailedEventHandler NavigationFailed;
#pragma warning restore 0067

		/// <summary>
		/// Go back to the last page
		/// </summary>
		public void GoBack()
		{
			GoBackPartial();
		}

		/// <summary>
		/// Go forward to the last page
		/// </summary>
		public void GoForward()
		{
			GoForwardPartial();
		}

		/// <summary>
		/// Loads the content at the specified uri as a new document.
		/// </summary>
		/// <param name="uri"></param>
		public void Navigate(Uri uri)
		{
			this.SetInternalSource(uri ?? BlankUri);
		}

		/// <summary>
		/// Loads the specified HTML content as a new document.
		/// </summary>
		/// <param name="text">The HTML content to display in the WebView control.</param>
		public void NavigateToString(string text)
		{
			this.SetInternalSource(text ?? "");
		}

		/// <summary>
		/// Loads the content at the specified HttpRequestMessage as a new document
		/// </summary>
		/// <param name="requestMessage"></param>
		/// <exception cref="ArgumentException"></exception>
		public void NavigateWithHttpRequestMessage(HttpRequestMessage requestMessage)
		{
			if (requestMessage?.RequestUri == null)
			{
				throw new ArgumentException("Invalid request message. It does not have a RequestUri.");
			}

			SetInternalSource(requestMessage);
		}

		/// <summary>
		/// Stop the loading process
		/// </summary>
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
