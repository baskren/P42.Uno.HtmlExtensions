using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using P42.Serilog.QuickLog;
using P42.Uno.Xaml.Controls;
using Uno.Extensions;
using Uno.Foundation;
using Uno.UI.Runtime.WebAssembly;

#if __WASM__


namespace Uno.UI.Tests;
//namespace P42.Uno.WebControls;

[HtmlElement("iframe")]
public partial class NativeWebView : FrameworkElement, Uno.UI.Xaml.Controls.INativeWebView
{
    #region Static Implementation

    #region Constants
    private const bool DebugMode = false;

    private static readonly Guid SessionGuid = Guid.NewGuid();

    private static string BridgeScript { get; set; } = string.Empty;
    private static string BridgePage { get; set; } = string.Empty;
    private static string BridgePageBase64 { get; set; } = string.Empty;
    #endregion

    #region Fields
    private static readonly Dictionary<string, WeakReference<NativeWebView>> Instances = new();
    private static readonly Dictionary<string, TaskCompletionSource<string>> TaskCompletionSources = new();
    #endregion
        
    #region Construction
    static NativeWebView()
    {
        DebugWriteLine($"NativeWebView.STATIC ENTER");
        WebAssemblyRuntime.InvokeJS($"sessionStorage.setItem('Uno.WebView.Session','{SessionGuid}');");
        //var windowLocationHref = WebAssemblyRuntime.InvokeJS("window.location.href");
        //DebugWriteLine($"NativeWebView.STATIC window.location.href: [{windowLocationHref}]");
        //var windowScriptDirectory = WebAssemblyRuntime.InvokeJS("window.scriptDirectory");
        //DebugWriteLine($"NativeWebView.STATIC window.scriptDirectory: [{windowScriptDirectory}]");

        #region Load Bridge
        var asm = typeof(NativeWebView).Assembly;
        if (EmbeddedResourceExtensions.ResourceAsString(".Resources.UnoWebViewBridge.js", asm) is { } bridgeScript)
            BridgeScript = $"<script>\n{bridgeScript}\n</script>";

        if (EmbeddedResourceExtensions.ResourceAsString(".Resources.UnoWebViewBridge.html", asm) is { } page)
        {
            BridgePage = InjectWebBridge(page);
            BridgePageBase64 = AsBase64Source(BridgePage);
        }
        #endregion

        // keep linker from overdoing it!
        OnFrameLoaded(null);
        OnMessageReceived(null);
        DebugWriteLine($"NativeWebView.STATIC EXIT");
    }
    
    #endregion

    #region Helper Functions
    private static string AsBase64Source(string source)
    {
        var valueBytes = Encoding.UTF8.GetBytes(source);
        var base64 = Convert.ToBase64String(valueBytes);
        return "data:text/html;charset=utf-8;base64," + base64;
    }

    private static string InjectWebBridge(string text)
    {
        var index = text.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (index > -1)
            return text.Insert(index, BridgeScript);

        index = text.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (index > -1)
            return text.Insert(index, BridgeScript);

        index = text.IndexOf("</html>", StringComparison.OrdinalIgnoreCase);
        if (index > -1)
            return text.Insert(index, BridgeScript);

        return text + BridgePage;
    }

    private static void DebugWriteLine(params object[] args)
    {
        if (DebugMode)
            Console.WriteLine(args);
        else
            Debug.WriteLine(args);
    }

    /*
    private static NativeWebView InstanceForGuid(string guid)
    {
        if (!Instances.TryGetValue(guid, out var weakRef))
            return null;

        return weakRef.TryGetTarget(out var nativeWebView) ? nativeWebView : null;
    }
    */
    
    
    #endregion

    #region Javascript-Page Event Handlers
    // Called on every page load ... even if the page isn't bridged
    private static void OnFrameLoaded(string? guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return;

        DebugWriteLine("NativeWebView.OnFrameLoaded ENTER");
        
        /*
        if (InstanceForGuid(guid) is NativeWebView { Parent:  parent })
        {
            parent.InternalSetCanGoBack(false);
            parent.InternalSetCanGoForward(false);
        }
        */
        DebugWriteLine("NativeWebView.OnFrameLoaded EXIT");
    }

    private static void OnMessageReceived(string? json)
    {
        DebugWriteLine("NativeWebView.OnMessageReceived ENTER");
        OnMessageReceivedInner(json);
        DebugWriteLine("NativeWebView.OnMessageReceived EXIT");
    }
    
    private static void OnMessageReceivedInner(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        var message = JObject.Parse(json);
        if (!message.TryGetValue("Target", out var target) || target.ToString() != SessionGuid.ToString())
            return;

        DebugWriteLine($"NativeWebView.OnMessageReceived Target:[{target}]");
        if (!message.TryGetValue("Method", out var method))
            return;

        DebugWriteLine($"NativeWebView.OnMessageReceived Method:[{method}]");
        switch (method.ToString())
        {
            case nameof(InvokeScriptAsync):
            case nameof(ExecuteScriptAsync):
                {
                    if (message.TryGetValue("TaskId", out var taskId) && 
                        TaskCompletionSources.TryGetValue(taskId.ToString(), out var tcs))
                    {
                        DebugWriteLine($"$NativeWebView.OnMessageReceived TaskId:[{taskId}]");
                        TaskCompletionSources.Remove(taskId.ToString());
                        if (message.TryGetValue("Result", out var result))
                            tcs.SetResult(result.ToString());
                        else if (message.TryGetValue("Error", out var error))
                            tcs.SetException(new Exception($"Javascript Error: [{error}]"));
                        else
                            tcs.SetException(new Exception("Javascript failed for unknown reason"));
                    }
                }
                break;
            case "OnBridgeLoaded":
                // called after bridged page is loaded
                {
                    DebugWriteLine("NativeWebView.OnMessageReceived : OnBridgeLoaded  ENTER");
                    OnBridgeLoadedInner(message);
                    DebugWriteLine("NativeWebView.OnMessageReceived : OnBridgeLoaded  EXIT");
                }
                break;
        }
    }

    // not sure how much of this will be needed .... 
    private static void OnBridgeLoadedInner(JObject? message)
    {
        /*
        if (!message.TryGetValue("Source", out var source))
            return;

        DebugWriteLine($"NativeWebView.OnMessageReceived Source: [{source}]");
        if (!Instances.TryGetValue(source.ToString(), out var weakReference) ||
            !weakReference.TryGetTarget(out var nativeWebView))
            return;

        DebugWriteLine($"NativeWebView[{nativeWebView._id}].NavigateToText: _bridgeConnected=[{nativeWebView._bridgeConnected}] _internalSource=[{nativeWebView._internalSource?.GetType()}] _raceSource=[{nativeWebView._raceSource?.GetType()}]");
        if (!nativeWebView._bridgeConnected)
        {
            nativeWebView._bridgeConnected = true;
            nativeWebView.UpdateFromInternalSource();
        }

        DebugWriteLine($"NativeWebView.OnMessageReceived nativeWebView[{nativeWebView._id}].Parent={nativeWebView.Parent}");
        if (nativeWebView.Parent is not WebViewX parent ||
            !message.TryGetValue("Pages", out var pages) || !int.TryParse(pages.ToString(), out var pageCount) ||
            !message.TryGetValue("Page", out var page) || !int.TryParse(page.ToString(), out var pageIndex))
            return;

        DebugWriteLine("NativeWebView.OnMessageReceived PARENT!");
        parent.InternalSetCanGoBack(pageIndex > 1);
        parent.InternalSetCanGoForward(pageCount > pageIndex);
        if (!message.TryGetValue("Href", out var hrefJObject))
            return;

        var href = hrefJObject.ToString();
        DebugWriteLine($"NativeWebView.OnMessageReceived Href:[{hrefJObject}]");
        Uri uri = null;
        if (href.StartsWith("http") || href.StartsWith("file"))
            uri = new Uri(href);
        else if (href.StartsWith("data"))
            uri = new Uri("data:");

        DebugWriteLine($"NativeWebView.OnMessageReceived uri={uri}");
        DebugWriteLine($"NativeWebView.OnMessageReceived href={href}");

        if (href == BridgePageBase64)
        {
            DebugWriteLine("NativeWebView.OnBridgeLoaded: PAGE IS BRIDGE PAGE");
            nativeWebView.Activated = true;
            nativeWebView.UpdateFromInternalSource();
        }
        else
        {
            DebugWriteLine($"NativeWebView.OnBridgeLoaded uri:[{uri}]");
            parent.OnNavigationCompleted(true, uri, Windows.Web.WebErrorStatus.Found);
        }


        if (nativeWebView._raceSource == null)
            return;

        nativeWebView._internalSource = nativeWebView._raceSource;
        nativeWebView._raceSource = null;
        nativeWebView.UpdateFromInternalSource();
        */
    }
    #endregion
        
    #endregion

    #region Fields
    private readonly string _id;
    private readonly Guid _instanceGuid;
    private bool _bridgeConnected;
    //internal bool Activated;
    private readonly CoreWebView2 _coreWebView;
    #endregion

    #region Construction / Initialization
    public NativeWebView(CoreWebView2 coreWebView)
    {
        _coreWebView = coreWebView;
        _instanceGuid = Guid.NewGuid();
        _id = this.GetHtmlAttribute("id");
        
        DebugWriteLine($"NativeWebView[{_id}].ctr ENTER : InstanceGuid=[{_instanceGuid}]");
        Instances.Add(_instanceGuid.ToString(), new WeakReference<NativeWebView>(this));
        this.SetCssStyle("border", "none");
        this.SetHtmlAttribute("name", $"{SessionGuid}:{_instanceGuid}");
        this.SetHtmlAttribute("onLoad", $"UnoWebView_OnLoad('{_instanceGuid}')");
        this.SetHtmlAttribute("src", BridgePageBase64);

        /*
        var cwvType = _coreWebView.GetType();
        var nwvFieldInfo = cwvType.GetField("_nativeWebView", BindingFlags.Instance | BindingFlags.NonPublic);
        nwvFieldInfo?.SetValue(_coreWebView, this);
        
        var updateFromInternalSourceMethod = cwvType.GetMethod("UpdateFromInternalSource", BindingFlags.Instance | BindingFlags.NonPublic);
        updateFromInternalSourceMethod?.Invoke(_coreWebView, new object[] { });
        
        var onScrollEnabledFieldInfo = cwvType.GetField("_onScrollEnabled", BindingFlags.Instance | BindingFlags.NonPublic);
        var onScrollEnabled = onScrollEnabledFieldInfo?.GetValue(_coreWebView);
        var onScrollEnabledChangedMethod = cwvType.GetMethod("OnScrollEnabledChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        onScrollEnabledChangedMethod?.Invoke(_coreWebView, new object[] { onScrollEnabled ?? false });
        */
        
        DebugWriteLine($"NativeWebView[{_id}].ctr EXIT");
    }
    
    
    #endregion

    private void UpdatePointerEvents()
    {
        DebugWriteLine("NativeWebView.UpdatePointerEvents  THIS : " + this);
        DebugWriteLine("NativeWebView.UpdatePointerEvents  PARENT : " + Parent);
            
        if (Parent is UIElement parent)
        {
            parent.SetCssStyle("pointer-events", "auto");
            if (parent.GetVisualTreeParent() is {} grandParent)
            {
                DebugWriteLine("NativeWebView.UpdatePointerEvents  GRAND PARENT : " + grandParent);
                grandParent.SetCssStyle("pointer-events", "auto");
            }
        }
        this.SetCssStyle("pointer-events", "auto");
    }


    #region INativeWebView
    public void GoBack()
    {
        if (!_bridgeConnected)
        {
            QLog.Warning(WasmWebViewBridgeNotConnectedException.DefaultMessage);
            return;
        }
        WebAssemblyRuntime.InvokeJS(new Message(this));
    }

    public void GoForward()
    {
        if (!_bridgeConnected)
        {
            QLog.Warning(WasmWebViewBridgeNotConnectedException.DefaultMessage);
            return;
        }
        WebAssemblyRuntime.InvokeJS(new Message(this));
    }

    public void Stop()
    {
        QLog.Warning("Stop() is not implemented in WASM implementation of WebView / WebView2");
    }

    public void Reload()
    {
        if (!_bridgeConnected)
        {
            QLog.Warning(WasmWebViewBridgeNotConnectedException.DefaultMessage);
            return;
        }
        WebAssemblyRuntime.InvokeJS(new Message(this));
    }

    public void ProcessNavigation(Uri uri)
    {
        if (!_bridgeConnected)
            return;
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation({uri}): ENTER");
        _bridgeConnected = false;
        UpdatePointerEvents();
        WebAssemblyRuntime.InvokeJS(new Message<Uri>(this, uri));
        UpdatePointerEvents();
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation({uri}): EXIT");
    }

    public void ProcessNavigation(string html)
    {
        if (!_bridgeConnected)
        {
            QLog.Warning(WasmWebViewBridgeNotConnectedException.DefaultMessage);
            return;
        }
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation(text): ENTER: {html.Substring(Math.Max(0, html.Length - 100), 100)}");
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation(text): _bridgeConnected=[{_bridgeConnected}] ");
        html = NativeWebView.InjectWebBridge(html);
        _bridgeConnected = false;
        var message = new Message<string>(this, AsBase64Source(html));
        var msg = message.ToString();
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation(text): message ::: {msg.Substring(0, Math.Min(100, msg.Length))}");
        UpdatePointerEvents();
        WebAssemblyRuntime.InvokeJS(message);
        UpdatePointerEvents();
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation(text: EXIT {html.Substring(Math.Max(0, html.Length - 100), 100)}");
    }

    public void ProcessNavigation(HttpRequestMessage requestMessage)
    {
        if (!_bridgeConnected)
        {
            QLog.Warning(WasmWebViewBridgeNotConnectedException.DefaultMessage);
            return;
        }
        
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation(RequestMessage)({requestMessage.RequestUri}): ENTER");
        _bridgeConnected = false;
        UpdatePointerEvents();
        WebAssemblyRuntime.InvokeJS(new NavigateRequestMessage(this, requestMessage));
        UpdatePointerEvents();
        DebugWriteLine($"NativeWebView[{_id}].ProcessNavigation(RequestMessage)({requestMessage.RequestUri}): EXIT");

    }

    // QUESTIONS
    // 1. Does InvokeAsync return right away (since the script is passed via the bridge to the iframe)
    // 2. If the cancellation token is called, is there a way we can abort the script in the iframe
    // 3. If the cancellation token is called, should we complete or cancel the TaskCompletionSource
    public async Task<string?> ExecuteScriptAsync(string script, CancellationToken token)
    {
        if (!_bridgeConnected)
            return await Task.FromException<string>(new WasmWebViewBridgeNotConnectedException());
        var tcs = new TaskCompletionSource<string>();
        var taskId = Guid.NewGuid().ToString();
        TaskCompletionSources.Add(taskId, tcs);
        try
        {
            await WebAssemblyRuntime.InvokeAsync(
                new ScriptMessage(this, taskId, script),
                token);
            return await tcs.Task;
        }
        catch (Exception ex)
        {
            TaskCompletionSources.Remove(taskId); 
            return await Task.FromException<string>(ex);
        }
    }

    public async Task<string?> InvokeScriptAsync(string functionName, string[]? arguments, CancellationToken token)
    {
        if (!_bridgeConnected)
            return await Task.FromException<string>(new WasmWebViewBridgeNotConnectedException());
        var tcs = new TaskCompletionSource<string>();
        var taskId = Guid.NewGuid().ToString();
        TaskCompletionSources.Add(taskId, tcs);
        try
        {
            await WebAssemblyRuntime.InvokeAsync(
                new ScriptFunctionMessage(this, taskId, functionName, arguments),
                token);
            return await tcs.Task;
        }
        catch (Exception ex)
        {
            TaskCompletionSources.Remove(taskId); 
            return await Task.FromException<string>(ex);
        }
    }

    public void SetScrollingEnabled(bool isScrollingEnabled)
    {
        if (isScrollingEnabled)
            this.SetCssStyle(("overflow", "hidden"),("height", "100%"), ("width", "100%"));
        else
            this.SetCssStyle(("overflow", "initial"), ("height", "auto"), ("width", "auto"));
    }
    #endregion
    
    #region WebBridge Message Classes
    internal class Message(
        NativeWebView nativeWebView,
        [System.Runtime.CompilerServices.CallerMemberName] string callerName = null!)
    {
        public string Source { get; private init; } = SessionGuid.ToString();

        public string Method { get; private init; } = callerName;

        public string Target { get; private init; } = nativeWebView._instanceGuid.ToString();

        [JsonIgnore]
        private string Id { get; } = nativeWebView._id;

        public override string ToString() => JsonConvert.SerializeObject(ToDictionary());

        public static implicit operator string(Message m) => $"UnoWebView_PostMessage('{m.Id}','{m}');";

        protected virtual Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { nameof(Source), Source },
                { nameof(Method), Method },
                { nameof(Target), Target }
            };
        }
    }

    internal class Message<T>(
        NativeWebView nativeWebView,
        T payload,
        [System.Runtime.CompilerServices.CallerMemberName] string callerName = null!)
        : Message(nativeWebView, callerName)
    {
        public T Payload { get; private init; } = payload;

        protected override Dictionary<string, object> ToDictionary()
        {
            var result = base.ToDictionary();
            if (Payload != null)
                result[nameof(Payload)] = Payload;
            return result;
        }
    }

    internal class NavigateRequestMessage(
        NativeWebView nativeWebView,
        HttpRequestMessage requestMessage,
        [System.Runtime.CompilerServices.CallerMemberName] string callerName = null!)
        : Message(nativeWebView, callerName)
    {

        private List<List<string>> Headers => requestMessage.Headers
            .Select(h => new List<string> { h.Key, h.Value.JoinBy(", ") })
            .ToList();
        
        protected override Dictionary<string, object> ToDictionary()
        {
            var result = base.ToDictionary();
            
            result[nameof(HttpRequestMessage.Version)] = requestMessage.Version;
            result[nameof(HttpRequestMessage.VersionPolicy)] = requestMessage.VersionPolicy;
            if (requestMessage.Content is not null)
                result[nameof(HttpRequestMessage.Content)] = requestMessage.Content;
            result["RequestMethod"] = requestMessage.Method.Method.ToUpperInvariant();
            if (requestMessage.RequestUri is not null)
                result[nameof(HttpRequestMessage.RequestUri)] = requestMessage.RequestUri;
            result[nameof(HttpRequestMessage.Headers)] = Headers;

            var options = new Dictionary<string, object>();
            foreach (var option in requestMessage.Options)
                if (option.Value != null)
                    options[option.Key] = option.Value;
            result[nameof(HttpRequestMessage.Options)] = options;
            
            return result;
        }
    }


    internal class ScriptMessage(
        NativeWebView nativeWebView,
        string taskId,
        string script,
        [System.Runtime.CompilerServices.CallerMemberName] string callerName = null!)
        : Message(nativeWebView, callerName)
    {
        public string Script { get; private init; } = script;

        public string TaskId { get; private init;} = taskId;

        protected override Dictionary<string, object> ToDictionary()
        {
            var result = base.ToDictionary();
            result[nameof(Script)] = Script;
            result[nameof(TaskId)] = TaskId;
            return result;
        }
    }


    internal class ScriptFunctionMessage(
        NativeWebView nativeWebView,
        string taskId,
        string functionName,
        string[]? arguments,
        [System.Runtime.CompilerServices.CallerMemberName] string callerName = null!)
        : Message<string[]?>(nativeWebView, arguments, callerName)
    {
        public string FunctionName { get; private init; } = functionName;

        public string TaskId { get; private init; } = taskId;
        
        

        protected override Dictionary<string, object> ToDictionary()
        {
            var result = base.ToDictionary();
            result[nameof(FunctionName)] = FunctionName;
            result[nameof(TaskId)] = TaskId;
            return result;
        }
    }
    #endregion
        
    
}


#endif
