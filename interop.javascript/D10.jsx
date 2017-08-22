//alert("Attempting to run D10. New dims are " + arguments[0] + " by " + arguments[1] + ". Conversion factor is " + arguments[2])

if (arguments[0]) {
    w = arguments[0] * arguments[2];
    h = arguments[1] * arguments[2];

    app.activeDocument.artboards[0].artboardRect = [0, 0, w, -h];
}


/*
if (Math.abs(app.activeDocument.artboards[0].artboardRect.width) == Math.abs(w) && Math.abs(app.activeDocument.artboards[0].artboardRect.width.height) == Math.abs(h)) {
    //
}
else {
    app.activeDocument.artboards[0].artboardRect = [0, 0, w, h]
}
*/
