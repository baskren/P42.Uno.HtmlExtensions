﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Uno.UI.Runtime.WebAssembly;
using Uno.Foundation;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace P42.Uno.HtmlExtensions
{
    [HtmlElement("iframe")]
    public partial class NativeWebView : FrameworkElement
    {
        static readonly Guid SessionGuid = Guid.NewGuid();
        static readonly string Location;
        static readonly string PackageLocation;

        static NativeWebView()
        {
            WebAssemblyRuntime.InvokeJS($"sessionStorage.setItem('Uno.WebView.Session','{SessionGuid}');");
            Location = WebAssemblyRuntime.InvokeJS("window.location.href");
            PackageLocation = WebAssemblyRuntime.InvokeJS("window.scriptDirectory");
            System.Diagnostics.Debug.WriteLine("NativeWebView.STATIC location: " + Location);
        }

        static Dictionary<string, WeakReference<NativeWebView>> Instances = new Dictionary<string, WeakReference<NativeWebView>>();
        static Dictionary<string, TaskCompletionSource<string>> TCSs = new Dictionary<string, TaskCompletionSource<string>>();

        static NativeWebView InstanceForGuid(string guid)
        {
            if (Instances.TryGetValue(guid, out var weakRef))
            {
                if (weakRef.TryGetTarget(out var nativeWebView))
                    return nativeWebView;
            }
            return null;
        }

        // Called on every page load ... even if the page isn't bridged
        public static void OnFrameLoaded(string guid)
        {
            System.Diagnostics.Debug.WriteLine("NativeWebView.OnFrameLoaded ENTER");
            if (InstanceForGuid(guid) is NativeWebView nativeWebView)
            {
                if (nativeWebView.Parent is WebViewX parent)
                {
                    parent.InternalSetCanGoBack(false);
                    parent.InternalSetCanGoForward(false);
                }
                nativeWebView.ClearCssStyle("pointer-events");
            }
            System.Diagnostics.Debug.WriteLine("NativeWebView.OnFrameLoaded EXIT");
        }

        public static void OnMessageReceived(string json)
        {
            var message = JObject.Parse(json);
            if (message.TryGetValue("Target", out var target) && target.ToString() == SessionGuid.ToString())
            {
                System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived Target:" + target.ToString());
                if (message.TryGetValue("Method", out var method))
                {
                    System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived Method:" + method.ToString());
                    switch (method.ToString())
                    {
                        case nameof(InvokeScriptAsync):
                            {
                                if (message.TryGetValue("TaskId", out var taskId) && 
                                    TCSs.TryGetValue(taskId.ToString(), out var tcs))
                                {
                                    System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived TaskId:" + taskId.ToString());
                                    TCSs.Remove(taskId.ToString());
                                    if (message.TryGetValue("Result", out var result))
                                        tcs.SetResult(result.ToString());
                                    else if (message.TryGetValue("Error", out var error))
                                        tcs.SetException(new Exception("Javascript Error: " + error.ToString()));
                                    else
                                        tcs.SetException(new Exception("Javascript failed for unknown reason"));
                                }
                            }
                            break;
                        case "OnBridgeLoaded":
                            // called after bridged page is loaded
                            {
                                System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived : OnBridgeLoaded  ENTER");
                                if (message.TryGetValue("Source", out var source))
                                {
                                    System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived Source:" + source.ToString());
                                    if (Instances.TryGetValue(source.ToString(), out var weakReference) &&
                                    weakReference.TryGetTarget(out var nativeWebView))
                                    {
                                        if (!nativeWebView._bridgeConnected)
                                        {
                                            nativeWebView._bridgeConnected = true;
                                            nativeWebView.UpdateFromInternalSource();
                                        }
                                        if (nativeWebView.Parent is WebViewX parent &&
                                            message.TryGetValue("Pages", out var pages) && int.TryParse(pages.ToString(), out var pageCount) &&
                                            message.TryGetValue("Page", out var page) && int.TryParse(page.ToString(), out var pageIndex))
                                        {
                                            parent.InternalSetCanGoBack(pageIndex > 1);
                                            parent.InternalSetCanGoForward(pageCount > pageIndex);
                                            if (message.TryGetValue("Href", out var hrefJObject))
                                            {
                                                var href = hrefJObject.ToString();
                                                System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived Href:" + hrefJObject.ToString());
                                                Uri uri = null;
                                                if (href.StartsWith("http") || href.StartsWith("file"))
                                                    uri = new Uri(href);
                                                else if (href.StartsWith("data"))
                                                    uri = new Uri("data:");
                                                if (href == WebViewBridgeRootPage)
                                                {
                                                    nativeWebView._activated = true;
                                                    nativeWebView.UpdateFromInternalSource();
                                                }   
                                                else
                                                {
                                                    System.Diagnostics.Debug.WriteLine("NativeWebView.OnBridgeLoaded uri:" + uri);
                                                    parent.OnNavigationCompleted(true, uri, Windows.Web.WebErrorStatus.Found);
                                                }
                                            }
                                        }
                                    }
                                }
                                System.Diagnostics.Debug.WriteLine("NativeWebView.OnMessageReceived : OnBridgeLoaded  EXIT");
                            }
                            break;
                    }
                }
            }
        }


        static string WebViewBridgeRootPage => PackageLocation + "Assets/UnoWebViewBridge.html";
        internal static string WebViewBridgeScriptUrl => PackageLocation + "UnoWebViewBridge.js";

        public readonly string Id;
        readonly Guid InstanceGuid;

        private object _internalSource;
        private bool _bridgeConnected;
        internal bool _activated;

        public NativeWebView()
        {
            InstanceGuid = Guid.NewGuid();
            Id = this.GetHtmlAttribute("id");
            Instances.Add(InstanceGuid.ToString(), new WeakReference<NativeWebView>(this));
            this.SetCssStyle("border", "none");
            //this.ClearCssStyle("pointer-events");  // doesn't seem to work here as it seems to get reset by Uno during layout.
            this.SetHtmlAttribute("name", $"{SessionGuid}:{InstanceGuid}");
            this.SetHtmlAttribute("onLoad", $"UnoWebView_OnLoad('{InstanceGuid}')");
            System.Diagnostics.Debug.WriteLine("NativeWebView.ctr WebViewRootPage: " + WebViewBridgeRootPage);
            this.SetHtmlAttribute("src", WebViewBridgeRootPage);
        }


        void Navigate(Uri uri)
        {
            _bridgeConnected = false;
            _internalSource = null;
            WebAssemblyRuntime.InvokeJS(new Message<Uri>(this, uri));
        }

        void NavigateToText(string text)
        {
            System.Diagnostics.Debug.WriteLine("NativeWebView.NavigateToText: text (before): " + text.Substring(0, Math.Min(256, text.Length)));
            text = WebViewXExtensions.InjectWebBridge(text);
            var valueBytes = Encoding.UTF8.GetBytes(text);
            var base64 = Convert.ToBase64String(valueBytes);
            _bridgeConnected = false;
            _internalSource = null;
            System.Diagnostics.Debug.WriteLine("NativeWebView.NavigateToText " + text.Substring(0,Math.Min(256,text.Length)));
            WebAssemblyRuntime.InvokeJS(new Message<string>(this, "data:text/html;charset=utf-8;base64," + base64));
        }

        internal void GoBack()
        {
            if (_bridgeConnected)
                WebAssemblyRuntime.InvokeJS(new Message(this));
        }

        internal void GoForward()
        {
            if (_bridgeConnected)
                WebAssemblyRuntime.InvokeJS(new Message(this));
        }

        void NavigateWithHttpRequestMessage(HttpRequestMessage message)
        {
            throw new NotSupportedException();
        }


        internal async Task<string> InvokeScriptAsync(string functionName, string[] arguments)
        {
            var tcs = new TaskCompletionSource<string>();
            var taskId = Guid.NewGuid().ToString();
            TCSs.Add(taskId, tcs);
            WebAssemblyRuntime.InvokeJS(new ScriptMessage(this, taskId, functionName, arguments));
            return await tcs.Task;
        }

        

        internal void SetInternalSource(object source)
        {
            _internalSource = source;
            UpdateFromInternalSource();
        }

        private void UpdateFromInternalSource()
        {
            if (_bridgeConnected && _activated)
            {
                if (_internalSource is Uri uri)
                {
                    Navigate(uri);
                    return;
                }
                if (_internalSource is string html)
                {
                    NavigateToText(html);
                }
                if (_internalSource is HttpRequestMessage message)
                {
                    NavigateWithHttpRequestMessage(message);
                }
            }
        }


        class Message
        {
            public string Source { get; private set; }

            public string Method { get; private set; }

            public string Target { get; private set; }

            [JsonIgnore]
            public string Id { get; private set; }

            public Message(NativeWebView nativeWebView, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
            {
                Source = SessionGuid.ToString();
                Id = nativeWebView.Id;
                Target = nativeWebView.InstanceGuid.ToString();
                Method = callerName;
            }

            public override string ToString() => JsonConvert.SerializeObject(this);

            public static implicit operator string(Message m) => $"UnoWebView_PostMessage('{m.Id}','{m}');";

        }

        class Message<T> : Message
        {
            public T Payload { get; private set; }

            public Message(NativeWebView nativeWebView, T payload, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) 
                : base(nativeWebView, callerName)
                => Payload = payload;
        }

        class ScriptMessage : Message<string[]>
        {
            public string FunctionName { get; private set; }

            public string TaskId { get; private set; }

            public ScriptMessage(NativeWebView nativeWebView, string taskId, string functionName, string[] arguments, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) 
                : base(nativeWebView, arguments, callerName)
            {
                FunctionName = functionName;
                TaskId = taskId;
            }
        }
    }
}
