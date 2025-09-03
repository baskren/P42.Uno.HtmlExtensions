function P42_EnableOnLoad(id) {
    //console.log("P42_EnableOnLoad ENTER");
    const element = document.getElementById(id);
    //console.log("P42_EnableOnLoad A");

    if (element) {
        //console.log("P42_EnableOnLoad B");
        element.onload = function () {
            console.log("ELEMENT ["+id+"] loaded : " + window.location.href);
            P42_NotifyOnLoad(id, window.location.href).then(r =>
            {
                console.log("P42_EnableOnLoad C : " + r);
            });
        }
        //console.log("P42_EnableOnLoad EXIT B");
        return "ok";
    }

    //console.log("P42_EnableOnLoad EXIT A");
    return "error";
}

async function P42_NotifyOnLoad(id, message) {
    //console.log("P42_NotifyOnLoad ENTER : " + window.location.href);
    globalThis.myExports = await Module.getAssemblyExports("P42.Uno.MarkdownExtensions");
    return globalThis.myExports.P42.Uno.WasmExtensions.OnLoad(id, message);
}
