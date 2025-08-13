using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Uno.UI.Runtime.WebAssembly;
using Uno.Foundation;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Diagnostics;

namespace P42.Uno.HtmlExtensions
{
    [HtmlElement("iframe")]
    public partial class NativeWebView : FrameworkElement
    {
        const bool debug = false;

        static readonly Guid SessionGuid = Guid.NewGuid();
        static readonly string Location;
        static readonly string PackageLocation;

        static string BridgeScript;
        static string BridgePage;
        static string BridgePageBase64;

        static void DebugWriteLine(params object[] args)
            => DWL(debug, args);

        static void DWL(bool doit, params object[] args)
        {
            if (doit)
                Console.WriteLine(args);
            else
                Debug.WriteLine(args);
        }

        static NativeWebView()
        {
            DebugWriteLine($"NativeWebView.STATIC ENTER");
            WebAssemblyRuntime.InvokeJS($"sessionStorage.setItem('Uno.WebView.Session','{SessionGuid}');");
            Location = WebAssemblyRuntime.InvokeJS("window.location.href");
            PackageLocation = WebAssemblyRuntime.InvokeJS("window.scriptDirectory");
            DebugWriteLine("NativeWebView.STATIC location: " + Location);


            LoadBridge();

            // keep linker from overdoing it!
            OnFrameLoaded(null);
            OnMessageReceived(null);
            DebugWriteLine($"NativeWebView.STATIC EXIT");
        }


        static void LoadBridge()
        {
            var asm = typeof(NativeWebView).Assembly;
            var resourceIds = asm.GetManifestResourceNames();

            var resourceId = resourceIds.FirstOrDefault(id => id.EndsWith(".Resources.UnoWebViewBridge.js"));
            //DebugWriteLine($"NativeWebView.LoadBridgeScript : resourceId = {resourceId}");
            using (var stream = asm.GetManifestResourceStream(resourceId))
            {
                using (var reader = new StreamReader(stream))
                {
                    var script = "<script>\n" +
                         reader.ReadToEnd() +
                        "\n</script>";
                    BridgeScript = script;

                }
            }
            //DebugWriteLine($"NativeWebView.LoadBridgeScript : BridgeScript = {BridgeScript}");

            resourceId = resourceIds.FirstOrDefault(id => id.EndsWith(".Resources.UnoWebViewBridge.html"));
            //DebugWriteLine($"NativeWebView.LoadBridgeScript : resourceId = {resourceId}");
            using (var stream = asm.GetManifestResourceStream(resourceId))
            {
                using (var reader = new StreamReader(stream))
                {
                    var page = reader.ReadToEnd();
                    //DebugWriteLine($"NativeWebView.LoadBridgeScript : page = {page}");
                    BridgePage = InjectWebBridge(page);
                }
            }
            //DebugWriteLine($"NativeWebView.LoadBridgeScript : BridgePage = {BridgePage}");
            BridgePageBase64 = AsBase64Source(BridgePage);
        }

        static string AsBase64Source(string source)
        {
            var valueBytes = Encoding.UTF8.GetBytes(source);
            var base64 = Convert.ToBase64String(valueBytes);
            return "data:text/html;charset=utf-8;base64," + base64;
        }

        internal static string InjectWebBridge(string text)
        {
            /*
            var script = "<script src='" +
                NativeWebView.WebViewBridgeScriptUrl +
                "'></script>";
            */
            bool edited = false;
            var index = text.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (index > -1)
            {
                text = text.Insert(index, BridgeScript);
                edited = true;
            }
            if (!edited)
            {
                index = text.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                if (index > -1)
                {
                    text = text.Insert(index, BridgeScript);
                    edited = true;
                }
            }
            if (!edited)
            {
                index = text.IndexOf("</html>", StringComparison.OrdinalIgnoreCase);
                if (index > -1)
                {
                    text = text.Insert(index, BridgeScript);
                    edited = true;
                }
            }
            if (!edited)
            {
                text += BridgeScript;
            }
            //System.Diagnostics.Debug.WriteLine("WebViewXExtensions. new text: " + text);
            return text;
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
            DebugWriteLine("NativeWebView.OnFrameLoaded ENTER");
            if (string.IsNullOrWhiteSpace(guid))
                return;

            if (InstanceForGuid(guid) is NativeWebView nativeWebView)
            {
                if (nativeWebView.Parent is WebViewX parent)
                {
                    parent.InternalSetCanGoBack(false);
                    parent.InternalSetCanGoForward(false);
                }
            }
            DebugWriteLine("NativeWebView.OnFrameLoaded EXIT");
        }

        public static void OnMessageReceived(string json)
        {
            DebugWriteLine("NativeWebView.OnMessageReceived ENTER");

            if (!string.IsNullOrWhiteSpace(json))
            {
                var message = JObject.Parse(json);
                if (message.TryGetValue("Target", out var target) && target.ToString() == SessionGuid.ToString())
                {
                    DebugWriteLine("NativeWebView.OnMessageReceived Target:" + target.ToString());
                    if (message.TryGetValue("Method", out var method))
                    {
                        DebugWriteLine("NativeWebView.OnMessageReceived Method:" + method.ToString());
                        switch (method.ToString())
                        {
                            case nameof(InvokeScriptAsync):
                            case nameof(InvokeScriptFunctionAsync):
                                {
                                    if (message.TryGetValue("TaskId", out var taskId) && 
                                        TCSs.TryGetValue(taskId.ToString(), out var tcs))
                                    {
                                        DebugWriteLine("NativeWebView.OnMessageReceived TaskId:" + taskId.ToString());
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
                                    DebugWriteLine($"NativeWebView.OnMessageReceived : OnBridgeLoaded  ENTER");
                                    if (message.TryGetValue("Source", out var source))
                                    {
                                        DebugWriteLine($"NativeWebView.OnMessageReceived Source: " + source.ToString());
                                        if (Instances.TryGetValue(source.ToString(), out var weakReference) &&
                                        weakReference.TryGetTarget(out var nativeWebView))
                                        {
                                            DebugWriteLine($"NativeWebView[{nativeWebView.Id}].NavigateToText: _bridgeConnected=[{nativeWebView._bridgeConnected}] _internalSource=[{nativeWebView._internalSource?.GetType()}] _raceSource=[{nativeWebView._raceSource?.GetType()}]");
                                            if (!nativeWebView._bridgeConnected)
                                            {
                                                nativeWebView._bridgeConnected = true;
                                                nativeWebView.UpdateFromInternalSource();
                                            }

                                            DebugWriteLine($"NativeWebView.OnMessageReceived nativeWebView[{nativeWebView.Id}].Parent={nativeWebView.Parent}");
                                            if (nativeWebView.Parent is WebViewX parent &&
                                                message.TryGetValue("Pages", out var pages) && int.TryParse(pages.ToString(), out var pageCount) &&
                                                message.TryGetValue("Page", out var page) && int.TryParse(page.ToString(), out var pageIndex))
                                            {
                                                DebugWriteLine($"NativeWebView.OnMessageReceived PARENT!");
                                                parent.InternalSetCanGoBack(pageIndex > 1);
                                                parent.InternalSetCanGoForward(pageCount > pageIndex);
                                                if (message.TryGetValue("Href", out var hrefJObject))
                                                {
                                                    var href = hrefJObject.ToString();
                                                    DebugWriteLine($"NativeWebView.OnMessageReceived Href:" + hrefJObject.ToString());
                                                    Uri uri = null;
                                                    if (href.StartsWith("http") || href.StartsWith("file"))
                                                        uri = new Uri(href);
                                                    else if (href.StartsWith("data"))
                                                        uri = new Uri("data:");

                                                    DebugWriteLine($"NativeWebView.OnMessageReceived uri={uri}");
                                                    DebugWriteLine($"NativeWebView.OnMessageReceived href={href}");

                                                    if (href == BridgePageBase64)
                                                    {
                                                        DebugWriteLine($"NativeWebView.OnBridgeLoaded: PAGE IS BRIDGE PAGE");
                                                        nativeWebView._activated = true;
                                                        nativeWebView.UpdateFromInternalSource();
                                                    }
                                                    else
                                                    {
                                                        DebugWriteLine($"NativeWebView.OnBridgeLoaded uri:" + uri);
                                                        parent.OnNavigationCompleted(true, uri, Windows.Web.WebErrorStatus.Found);
                                                    }


                                                    if (nativeWebView._raceSource != null)
                                                    {
                                                        nativeWebView._internalSource = nativeWebView._raceSource;
                                                        nativeWebView._raceSource = null;
                                                        nativeWebView.UpdateFromInternalSource();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    DebugWriteLine($"NativeWebView.OnMessageReceived : OnBridgeLoaded  EXIT");
                                }
                                break;
                        }
                    }
                }

            }
            DebugWriteLine($"NativeWebView.OnMessageReceived EXIT");

        }

        public readonly string Id;
        readonly Guid InstanceGuid;

        private object _internalSource;
        private object _raceSource;
        private bool _bridgeConnected;
        internal bool _activated;

        public NativeWebView()
        {
            InstanceGuid = Guid.NewGuid();
            Id = this.GetHtmlAttribute("id");
            DebugWriteLine($"NativeWebView[{Id}].ctr ENTER : InstanceGuid=[{InstanceGuid}]");
            Instances.Add(InstanceGuid.ToString(), new WeakReference<NativeWebView>(this));
            this.SetCssStyle("border", "none");
            this.SetHtmlAttribute("name", $"{SessionGuid}:{InstanceGuid}");
            this.SetHtmlAttribute("onLoad", $"UnoWebView_OnLoad('{InstanceGuid}')");
            this.SetHtmlAttribute("src", BridgePageBase64);
            DebugWriteLine($"NativeWebView[{Id}].ctr EXIT");
        }

        void UpdatePointerEvents()
        {
            DebugWriteLine("NativeWebView.UpdatePointerEvents  THIS : " + this);


            DebugWriteLine("NativeWebView.UpdatePointerEvents  PARENT : " + Parent);
            if (Parent is UIElement parent)
            {
                parent.SetCssStyle("pointer-events", "auto");

                if (parent.GetVisualTreeParent() is UIElement grandParent)
                {
                    DebugWriteLine("NativeWebView.UpdatePointerEvents  GRAND PARENT : " + grandParent);
                    grandParent.SetCssStyle("pointer-events", "auto");
                }
            }
            this.SetCssStyle("pointer-events", "auto");
        }

        void Navigate(Uri uri)
        {
            DebugWriteLine($"NativeWebView[{Id}].Nativate({uri}): ENTER");
            _bridgeConnected = false;
            _internalSource = null;
            UpdatePointerEvents();
            WebAssemblyRuntime.InvokeJS(new Message<Uri>(this, uri));
            UpdatePointerEvents();
            DebugWriteLine($"NativeWebView[{Id}].Nativate({uri}): EXIT");
        }

        void NavigateToText(string text)
        {
            DebugWriteLine($"NativeWebView[{Id}].NavigateToText: ENTER: {text.Substring(Math.Max(0, text.Length - 100), 100)}");
            DebugWriteLine($"NativeWebView[{Id}].NavigateToText: _bridgeConnected=[{_bridgeConnected}] _internalSource=[{_internalSource?.GetType()}]");
            text = NativeWebView.InjectWebBridge(text);
            _bridgeConnected = false;
            _internalSource = null;
            var message = new Message<string>(this, AsBase64Source(text));
            var msg = message.ToString();
            DebugWriteLine($"NativeWebView[{Id}].NavigateToText: message ::: {msg.Substring(0, Math.Min(100, msg.Length))}");
            UpdatePointerEvents();
            WebAssemblyRuntime.InvokeJS(message);
            UpdatePointerEvents();
            DebugWriteLine($"NativeWebView[{Id}].NavigateToText: EXIT {text.Substring(Math.Max(0, text.Length - 100), 100)}");
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


        #region JavaScript
        internal async Task<string> InvokeScriptFunctionAsync(string functionName, string[] arguments)
        {
            var tcs = new TaskCompletionSource<string>();
            var taskId = Guid.NewGuid().ToString();
            TCSs.Add(taskId, tcs);
            WebAssemblyRuntime.InvokeJS(new ScriptFunctionMessage(this, taskId, functionName, arguments));
            return await tcs.Task;
        }

        internal async Task<string> InvokeScriptAsync(string script)
        {
            var tcs = new TaskCompletionSource<string>();
            var taskId = Guid.NewGuid().ToString();
            TCSs.Add(taskId, tcs);
            WebAssemblyRuntime.InvokeJS(new ScriptMessage(this, taskId, script));
            return await tcs.Task;
        }
        #endregion


        internal void SetInternalSource(object source)
        {
            DebugWriteLine($"NativeWebView[{Id}].SetInternalSource(object source)");
            DebugWriteLine($"NativeWebView[{Id}].SetInternalSource(object source): _bridgeConnected=[{_bridgeConnected}] _internalSource=[{_internalSource?.GetType()}]");
            _raceSource = _internalSource = source;
            UpdateFromInternalSource();
        }

        private void UpdateFromInternalSource()
        {
            DebugWriteLine($"NativeWebView[{Id}].wasm UpdateFromInternalSource ENTER : _bridgeConnected[{_bridgeConnected}] _activated[{_activated}]");

            if (_bridgeConnected && _activated)
            {
                if (_internalSource is Uri uri)
                {
                    DebugWriteLine($"\t\t Uri {uri}");
                    if (uri.IsFile)
                    {
                        DebugWriteLine($"\t\t Uri.IsFile = true");
                        try
                        {
                            var text = System.IO.File.ReadAllText(uri.AbsolutePath);
                            NavigateToText(text);
                        }
                        catch (Exception)
                        {
                            Navigate(uri);
                        }
                    }
                    else
                    {
                        DebugWriteLine($"\t\t Uri.IsFile = false");
                        Navigate(uri);
                    }
                }
                else if (_internalSource is string html)
                {
                    DebugWriteLine($"NativeWebView[{Id}].wasm UpdateFromInternalSource TEXT");
                    NavigateToText(html);
                }
                else if (_internalSource is HttpRequestMessage message)
                {
                    DebugWriteLine($"\t\t HttpRequestMessage");
                    NavigateWithHttpRequestMessage(message);
                }
                else
                {
                    DebugWriteLine($"\t\t Unsupported _internalSource Type: {_internalSource?.GetType()}");
                }
            }
            DebugWriteLine($"NativeWebView[{Id}].wasm UpdateFromInternalSource EXIT");
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

            public override string ToString() => JsonConvert.SerializeObject(ToDictionary());

            public static implicit operator string(Message m) => $"UnoWebView_PostMessage('{m.Id}','{m}');";

            public virtual Dictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>
                {
                    { nameof(Source), Source },
                    { nameof(Method), Method },
                    { nameof(Target), Target }
                };
            }
        }

        class Message<T> : Message
        {
            public T Payload { get; private set; }

            public Message(NativeWebView nativeWebView, T payload, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) 
                : base(nativeWebView, callerName)
                => Payload = payload;

            public override Dictionary<string, object> ToDictionary()
            {
                var result = base.ToDictionary();
                result[nameof(Payload)] = Payload;
                return result;
            }
        }

        class ScriptMessage : Message
        {
            public string Script { get; private set; }

            public string TaskId { get; private set;}

            public ScriptMessage(NativeWebView nativeWebView, string taskId, string script, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
                : base(nativeWebView, callerName)
            {
                Script = script;
                TaskId = taskId;
            }

            public override Dictionary<string, object> ToDictionary()
            {
                var result = base.ToDictionary();
                result[nameof(Script)] = Script;
                result[nameof(TaskId)] = TaskId;
                return result;
            }
        }


        class ScriptFunctionMessage : Message<string[]>
        {
            public string FunctionName { get; private set; }

            public string TaskId { get; private set; }

            public ScriptFunctionMessage(NativeWebView nativeWebView, string taskId, string functionName, string[] arguments, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null) 
                : base(nativeWebView, arguments, callerName)
            {
                FunctionName = functionName;
                TaskId = taskId;
            }

            public override Dictionary<string, object> ToDictionary()
            {
                var result = base.ToDictionary();
                result[nameof(FunctionName)] = FunctionName;
                result[nameof(TaskId)] = TaskId;
                return result;
            }
        }
    }
}
