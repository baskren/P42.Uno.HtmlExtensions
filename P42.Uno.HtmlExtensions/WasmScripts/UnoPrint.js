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