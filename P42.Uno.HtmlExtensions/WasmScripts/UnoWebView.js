function UnoWebView_createUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

var UnoWebView_InstanceGuid = UnoWebView_createUUID();
var UnoWebView_MessageListenerSet = false;

//console.log('UnoWebView.js [' + UnoWebView_InstanceGuid +'] LOADING ENTER');


function UnoWebView_PostMessage(id, message) {
    //console.log('UnoWebView[' + UnoWebView_InstanceGuid +']_PostMessage ENTER [' + id + ']: ' + message);
    let m = JSON.parse(message);
    let target = document.getElementById(id);
    target.contentWindow.postMessage(m, "*");
    //console.log('UnoWebView[' + UnoWebView_InstanceGuid +']_PostMessage EXIT [' + id + ']: ' + message);
}

function UnoWebView_SetMessageListener() {
    if (UnoWebView_MessageListenerSet) {
        //console.log('UnoWebView[' + UnoWebView_InstanceGuid + ']_SetMessageListener: ALREADY SET');
        return;
    }
    window.addEventListener("message", (event) => {
        UnoWebView_MessageListenerSet = true;
        let ignore = false;
        if (typeof event.data === "string" || event.data instanceof String) {
            ignore = event.data.toString().startsWith("setImmediate");
        }
        if (!ignore) {
            //console.log('UnoWebView[' + UnoWebView_InstanceGuid +']: messageListener ENTER : ' + JSON.stringify(event.data));
            //console.log('UnoWebView[' + UnoWebView_InstanceGuid +']: messageListener href: ' + window.location.href);
            if (event.data.Target == sessionStorage.getItem('Uno.WebView.Session')) {
                const OnMessageReceived = Module.mono_bind_static_method("[P42.Uno.HtmlExtensions] P42.Uno.HtmlExtensions.NativeWebView:OnMessageReceived");
                var json = JSON.stringify(event.data);
                OnMessageReceived(json);
            }
            //console.log('UnoWebView[' + UnoWebView_InstanceGuid +']: messageListener EXIT : ' + JSON.stringify(event.data));
        }
    }, false);
}

function UnoWebView_OnLoad(index) {
    //console.log('UnoWebView_OnLoad[' + UnoWebView_InstanceGuid +']: ENTER ' + index);
    UnoWebView_SetMessageListener();
    const OnFrameLoaded = Module.mono_bind_static_method("[P42.Uno.HtmlExtensions] P42.Uno.HtmlExtensions.NativeWebView:OnFrameLoaded");
    OnFrameLoaded(index);
    //console.log('UnoWebView_OnLoad[' + UnoWebView_InstanceGuid +']: EXIT ' + index);
}

//console.log('UnoWebView.js [' + UnoWebView_InstanceGuid +'] LOADING EXIT');
