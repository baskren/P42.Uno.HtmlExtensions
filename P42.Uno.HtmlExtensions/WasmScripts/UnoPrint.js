function UnoPrint_PrintElement(id) {
    let element = document.getElementById(id);

    let objFra = document.createElement('iframe');
    objFra.style.visibility = 'hidden';
    objFra.src = element.innerHTML;
    document.body.appendChild(objFra); 
    objFra.contentWindow.focus();
    objFra.contentWindow.print();
    document.body.removeChild(objFra);
}

function P42UnoPrint(html) {
    const hideFrame = document.createElement("iframe");
    hideFrame.onload = P42UnoSetPrint;
    hideFrame.style.position = "fixed";
    hideFrame.style.right = "0";
    hideFrame.style.bottom = "0";
    hideFrame.style.width = "0";
    hideFrame.style.height = "0";
    hideFrame.style.border = "0";
    hideFrame.srcdoc = html;
    document.body.appendChild(hideFrame);
}

function P42UnoClosePrint() {
    document.body.removeChild(this.__container__);
}

function P42UnoSetPrint() {
    this.contentWindow.__container__ = this;
    this.contentWindow.onbeforeunload = P42UnoClosePrint;
    this.contentWindow.onafterprint = P42UnoClosePrint;
    this.contentWindow.focus(); // Required for IE
    this.contentWindow.print();
}

