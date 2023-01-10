let UnoWebViewBridge_Debug = false;

if (window.parent !== window) {

    function createUUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    var InstanceGuid = createUUID();

    function GetGuids() {
        var session = "";
        var instance = "";
        if (window.name !== undefined && window.name !== null) {
            var guids = window.name.split(':');
            session = guids[0];
            if (guids.length > 1)
                instance = guids[1];
        }
        return { session: session, instance: instance };
    }

    function UnoWebViewBridge_PostMessage(message) {
        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_PostMessage ENTER message: ' + JSON.stringify(message));
        let guids = GetGuids();
        message.Target = guids.session;
        message.Source = guids.instance;
        message.InstanceGuid = InstanceGuid;
        window.parent.postMessage(message, "*");
        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_PostMessage EXIT message: ' + JSON.stringify(message));
    }

    function UnoWebViewBridge_ReceiveMessage(event) {
        var guids = GetGuids();

        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : event.data ' + event.data);
        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : event.data.Method ' + event.data.Method);
        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : event.data.Source ' + event.data.Source);
        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : guids.session ' + guids.session);
        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : event.data.Target ' + event.data.Target);
        if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : guids.instance ' + guids.instance);


        if (event.data.Source == guids.session && event.data.Target == guids.instance) {

            if (event.data.Method == 'Navigate') {
                if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : Navigate ' + event.data.Payload.substring(0, 100));
                window.location.assign(event.data.Payload);
                return;
            }
            else if (event.data.Method == 'NavigateToText') {
                if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + ']_ReceiveMessage : NavigateToText ' + event.data.Payload.substring(0, 100));
                window.location.assign(event.data.Payload);
                return;
            }
            else if (event.data.Method == 'Reload') {
                window.location.reload(event.data.Payload);
                return;
            }
            else if (event.data.Method == 'GoForward') {
                window.history.forward();
            }
            else if (event.data.Method == 'GoBack') {
                window.history.back();
            }
            else if (event.data.Method == 'InvokeScriptAsync') {
                try {
                    let script = event.data.Script;
                    if (UnoWebViewBridge_Debug) console.log('script: ' + script);
                    var result = eval(script);
                    if (UnoWebViewBridge_Debug) console.log('result: ' + result);
                    if (result === undefined || result === null)
                        result = "";
                    UnoWebViewBridge_PostMessage({ Method: event.data.Method, TaskId: event.data.TaskId, Result: result });
                } catch (error) {
                    UnoWebViewBridge_PostMessage({ Method: event.data.Method, TaskId: event.data.TaskId, Error: error });
                }
                return;
            }
            else if (event.data.Method == 'InvokeScriptFunctionAsync') {
                try {
                    let args = "";
                    if (event.data.Payload !== undefined && event.data.Payload !== null)
                        args = event.data.Payload.join();
                    let script = event.data.FunctionName + '(' + args + ');'
                    if (UnoWebViewBridge_Debug) console.log('script: ' + script);
                    var result = eval(script);
                    if (UnoWebViewBridge_Debug) console.log('result: ' + result);
                    if (result === undefined || result === null)
                        result = "";
                    UnoWebViewBridge_PostMessage({ Method: event.data.Method, TaskId: event.data.TaskId, Result: result });
                } catch (error) {
                    UnoWebViewBridge_PostMessage({ Method: event.data.Method, TaskId: event.data.TaskId, Error: error });
                }
                return;
            }

            // note that navigation echo messages never get sent
            UnoWebViewBridge_PostMessage({ Method: "echo", Arguments: [event.data.Method] });
        }
        else {
            if (UnoWebViewBridge_Debug) console.log('unknown message [' + InstanceGuid + ']: ' + JSON.stringify(event.data));
        }
    }

    function UnoWebViewBridge_InvokeScriptAsync(json) {
        try {

        } catch (error) {

        }
    }

    function UnoWebViewBridge_Initiate() {

        const currentWindowOnLoad = window.onload;
        window.onload = function () {

            if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + '] OnLoad ENTER : ' + window.location.href);

            let title = "";
            if (document.title !== undefined && document.title !== null)
                title = document.title;

            if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + '] Pages: ' + window.history.length);
            if (!history.state && typeof (history.replaceState) == "function")
                history.replaceState({ page: history.length, href: location.href, title: title }, title);
            if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + '] Pages: ' + window.history.length + '  Page: ' + window.history.state.page);

            if (currentWindowOnLoad !== undefined && currentWindowOnLoad !== null)
                currentWindowOnLoad();

            window.addEventListener("message", UnoWebViewBridge_ReceiveMessage, false);

            UnoWebViewBridge_PostMessage({ Method: "OnBridgeLoaded", InstanceGuid: InstanceGuid, Pages: window.history.length, Page: window.history.state.page, Href: window.location.href });

            if (UnoWebViewBridge_Debug) console.log('UnoWebViewBridge.js[' + InstanceGuid + '] OnLoad EXIT : ' + window.location.href);
        }
    }

    UnoWebViewBridge_Initiate();
}
