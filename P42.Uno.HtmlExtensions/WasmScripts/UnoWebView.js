let UnoWebView_Debug = false;

function UnoWebView_createUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

var UnoWebView_InstanceGuid = UnoWebView_createUUID();
var UnoWebView_MessageListenerSet = false;

if (UnoWebView_Debug) console.log('UnoWebView.js [' + UnoWebView_InstanceGuid +'] LOADING ENTER');


function UnoWebView_PostMessage(id, message) {
    if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid +']_PostMessage ENTER [' + id + ']: ' + message.substring(0,100));
    let m = JSON.parse(message);
    let target = document.getElementById(id);
    target.contentWindow.postMessage(m, "*");
    if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid +']_PostMessage EXIT [' + id + ']: ' + message.substring(0,100));
}

function UnoWebView_SetMessageListener() {
    if (UnoWebView_MessageListenerSet) {
        if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid + ']_SetMessageListener: ALREADY SET');
        return;
    }
    window.addEventListener("message", (event) => {
        UnoWebView_MessageListenerSet = true;
        let ignore = false;
        if (typeof event.data === "string" || event.data instanceof String) {
            ignore = event.data.toString().startsWith("setImmediate");
        }
        if (!ignore) {
            if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid +']: messageListener ENTER : ' + JSON.stringify(event.data));
            if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid +']: messageListener href: ' + window.location.href);
            if (event.data.Target == sessionStorage.getItem('Uno.WebView.Session')) {
                const OnMessageReceived = Module.mono_bind_static_method("[P42.Uno.HtmlExtensions.Wasm] P42.Uno.HtmlExtensions.NativeWebView:OnMessageReceived");
                var json = JSON.stringify(event.data);
                OnMessageReceived(json);
            }
            if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid +']: messageListener EXIT : ' + JSON.stringify(event.data));
        }
    }, false);
}

function UnoWebView_OnLoad(index) {
    if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid +']_OnLoad: ENTER ' + index);
    UnoWebView_SetMessageListener();
    const OnFrameLoaded = Module.mono_bind_static_method("[P42.Uno.HtmlExtensions.Wasm] P42.Uno.HtmlExtensions.NativeWebView:OnFrameLoaded");
    OnFrameLoaded(index);
    if (UnoWebView_Debug) console.log('UnoWebView.js[' + UnoWebView_InstanceGuid +']_OnLoad: EXIT ' + index);
}

if (UnoWebView_Debug) console.log('UnoWebView.js [' + UnoWebView_InstanceGuid +'] LOADING EXIT');
