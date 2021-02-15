require([`${config.uno_app_base}/html2canvas.min`], c => window.html2canvas = c);

function UnoPng_GetUrlPromise(id, width) {
    return new Promise(function (resolve, reject) {
        //var canvasResult = html2canvas(document.body);
        let element = document.body;
        if (id !== undefined && typeof id !== 'undefined') {
            element = document.getElementById(id);
        }
        let options = new Object();
        if (width !== undefined && typeof id !== 'undefined') {
            options.width = width;
        }
        var canvasResult = html2canvas(element, options);

        canvasResult.then(function (canvas) {

            let jepgPath = UnoPng_CreateBase64Image(canvas, "image/jpeg");
            let pngPath = UnoPng_CreateBase64Image(canvas, "image/png");
            resolve('success: true, Width: ' + canvas.width + ', Height: ' + canvas.height + ', PngPath: ' + pngPath + ", JpegPath: " + jepgPath);
        });

        canvasResult.catch(function (reason) {
            console.log('E.1');
            reject('success: false, reason: ' + reason);
        });

    });
}

function UnoPng_CreateBase64Image(canvas, mimeType) {
    let urlImage = canvas.toDataURL(mimeType);
    let base64Image = urlImage.replace("data:" + mimeType + ";base64,", "");
    let suffix = mimeType.replace("image/", "");
    let path = '/tmp/' + uuidv4() + "." + suffix;
    FS.writeFile(path, base64Image);
    return path;
}
