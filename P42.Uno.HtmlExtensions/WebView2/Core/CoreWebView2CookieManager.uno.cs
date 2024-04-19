using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uno;
using Windows.Foundation.Metadata;

#if __WASM__
using BaseWebView = P42.Uno.HtmlExtensions.WebViewX;
#else
using BaseWebView = Microsoft.UI.Xaml.Controls.WebView;
#endif

namespace P42.Web.WebView2.Core
{
    public class CoreWebView2CookieManager
    {
        #region Fields
        BaseWebView _webView;
        #endregion


        #region Construction
        internal CoreWebView2CookieManager(BaseWebView webView)
        {
            _webView = webView;
        }
        #endregion


        #region Methods
        public P42.Web.WebView2.Core.CoreWebView2Cookie CreateCookie(string name, string value, string Domain, string Path)
        {
            return new P42.Web.WebView2.Core.CoreWebView2Cookie
            {
                Name = name,
                Value = value,
                Domain = Domain,
                Path = Path
            };
        }

        public P42.Web.WebView2.Core.CoreWebView2Cookie CopyCookie(CoreWebView2Cookie cookie)
            => cookie.Clone();

        public void AddOrUpdateCookie(P42.Web.WebView2.Core.CoreWebView2Cookie cookie)
        {
#if !NET7_0_OR_GREATER && !__WASM__
            Task.Run(async () =>
            {
                var cmd = "document.cookie=" + cookie.ToString();
                var result = await _webView.InvokeScriptAsync(cmd, new string[] { });
            });
#endif
        }

        public void DeleteCookie(P42.Web.WebView2.Core.CoreWebView2Cookie cookie)
        {
            var del = cookie.Clone();
            del.Expired = true;
            AddOrUpdateCookie(del);
        }

        public void DeleteCookies(string name, string uri)
        {
            if (!uri.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !uri.StartsWith("file", StringComparison.OrdinalIgnoreCase))
                uri = "http://" + uri;
            var locator = new Uri(uri);
            DeleteCookiesWithDomainAndPath(name, locator.Host, locator.AbsolutePath);
        }

        public void DeleteCookiesWithDomainAndPath(string name, string Domain, string Path)
        {
            var del = new P42.Web.WebView2.Core.CoreWebView2Cookie
            {
                Name = name,
                Value = string.Empty,
                Domain = Domain,
                Path = Path
            };
            DeleteCookie(del);
        }

        public void DeleteAllCookies()
        {
#if !NET7_0_OR_GREATER && !__WASM__
            Task.Run(async () =>
            {
                var response = await _webView.InvokeScriptAsync("document.cookie", new string[] { });
                if (string.IsNullOrWhiteSpace(response))
                    return;

                var lines = response.Split(';');
                var names = new List<string>();
                foreach (var line in lines)
                {
                    var kvp = line.Split('=');
                    if (kvp.Length < 2)
                        continue;

                    var name = kvp[0].Trim();
                    var cookie = new CoreWebView2Cookie
                    {
                        Name = name,
                        Expired = true
                    };
                    var cmd = "document.cookie=" + cookie.ToString();
                    var result = await _webView.InvokeScriptAsync(cmd, new string[] { });
                }
            });
#endif
        }
#endregion

    }
}
