try {

    //alert("Attempting to run G30");

    //Transformation events also trigger object removal/addition events, based on the type of transformation.
    //If I did my homework, running G20 then G10 like normal should work.

    /////G20

    var doc = app.activeDocument;

    var RdataFile = new File(arguments[2] + "G21." + arguments[1] + ".NATA");

    var tolerance = 5;
    var conversion = arguments[3];

    var bugCounter = 0;

    RdataFile.open("r");

    var data = RdataFile.read().split("\n");

    RdataFile.close();

    if (data[0] == "empty") {
        //Do nothing.
    }
    else {
        for (i = 0, len = data.length - 1; i < len; i++) {
            doc.pageItems.getByName(data[i]).remove();
        }
    }


    /////G10

    //alert("Attempting to run G10");

    //Passed arguments:
    //0 - Path to javascript directory.
    //1 - Current runtime id.
    //2 - Path to document NATA.
    //3 - conversion ratio

    var doc = app.activeDocument;

    var dataFile = new File(arguments[2] + "G11." + arguments[1] + ".NATA");

    var tolerance = 5;
    var conversion = arguments[3];

    dataFile.open("r");

    var data = dataFile.read().split("\n");

    dataFile.close();

    if (data[0] == "empty") {
        //Do nothing.
    }
    else {

        //alert("Made it here: " + data[0]);

        //Geometry NATA structure:
        //0 - Type
        //  0 - Linear curve
        //  1 - Linear polyline
        //  2 - Any non-linear curve
        //1 - GUID
        //2 - Layer
        //3 - Span Count
        //4 - Coordinates (delimited by ,)

        for (i = 0, len = data.length; i < len - 1; i++) {
            var dataset = data[i].split("|");
            var type = parseInt(dataset[0]);
            var guid = dataset[1]
            var layer = dataset[2]
            var spans = parseInt(dataset[3]);

            bugCounter++;

            //Verify layer existence, or create.
            try {
                var activeLayer = app.activeDocument.layers.getByName(layer)
                //$.writeln("Success!");
            }
            catch (e) {
                var newLayer = doc.layers.add();
                newLayer.name = layer;
                var activeLayer = newLayer;
                //$.writeln("Failure!")
            }
            finally {

                //Create geometry.
                if (type == 0) {
                    //Convert linear curve to illustrator.
                    var points = dataset[4].split(",");

                    var x1 = points[0].substring(0, tolerance) * conversion;
                    var y1 = points[1].substring(0, tolerance) * conversion;
                    var x2 = points[2].substring(0, tolerance) * conversion;
                    var y2 = points[3].substring(0, tolerance) * conversion;

                    var newPath = doc.pathItems.add();

                    var newPoint_0 = newPath.pathPoints.add();
                    newPoint_0.anchor = [x1, y1];
                    newPoint_0.leftDirection = newPoint_0.anchor;
                    newPoint_0.rightDirection = newPoint_0.anchor;
                    newPoint_0.pointType = PointType.CORNER;

                    var newPoint_1 = newPath.pathPoints.add();
                    newPoint_1.anchor = [x2, y2];
                    newPoint_1.leftDirection = newPoint_1.anchor;
                    newPoint_1.rightDirection = newPoint_1.anchor;
                    newPoint_1.pointType = PointType.CORNER;

                    newPath.stroked = true;

                    app.redraw();

                    newPath.name = guid;
                    newPath.move(activeLayer, ElementPlacement.PLACEATEND);
                }

                else if (type == 1) {

                    var points = dataset[4].split(",");

                    var newPath = doc.pathItems.add();

                    for (c = 0, len2 = spans + 1; c < len2; c++) {
                        //Iterate through all points.
                        var x = points[c * 2].substring(0, tolerance) * conversion;
                        var y = points[(c * 2) + 1].substring(0, tolerance) * conversion;

                        var newPoint = newPath.pathPoints.add();
                        newPoint.anchor = [x, y];
                        newPoint.leftDirection = newPoint.anchor;
                        newPoint.rightDirection = newPoint.anchor;
                        newPoint.pointType = PointType.CORNER;
                    }

                    newPath.stroked = true;
                    newPath.filled = false;

                    app.redraw();

                    newPath.name = guid;
                    newPath.move(activeLayer, ElementPlacement.PLACEATEND);
                }

                else if (type == 2) {
                    var allPoints = dataset[4].split(";");

                    var newPath = doc.pathItems.add();

                    for (c = 0, len2 = allPoints.length; c < len2; c++) {
                        var newPointData = allPoints[c].split(":");
                        //Curve data structure, newPointData[]:
                        //0 - anchorX
                        //1 - anchorY
                        //2 - leftDirX
                        //3 - leftDirY
                        //4 - rightDirX
                        //5 - rightDirY

                        var anchorX = newPointData[0].substring(0, tolerance) * conversion;
                        var anchorY = newPointData[1].substring(0, tolerance) * conversion;
                        var leftDirX = newPointData[2].substring(0, tolerance) * conversion;
                        var leftDirY = newPointData[3].substring(0, tolerance) * conversion;
                        var rightDirX = newPointData[4].substring(0, tolerance) * conversion;
                        var rightDirY = newPointData[5].substring(0, tolerance) * conversion;

                        var newPoint = newPath.pathPoints.add();

                        newPoint.anchor = [anchorX, anchorY];
                        newPoint.leftDirection = [leftDirX, leftDirY];
                        newPoint.rightDirection = [rightDirX, rightDirY];

                        if (c == 0 || c == len2 - 1) {
                            newPoint.pointType = PointType.CORNER;
                        }
                        else {
                            newPoint.pointType = PointType.CORNER;
                        }

                    }

                    newPath.stroked = true;
                    newPath.filled = false;

                    //Redraw after all points added.
                    app.redraw();

                    newPath.name = guid;
                    newPath.move(activeLayer, ElementPlacement.PLACEATEND);
                }

            } //finally

        }

    }//else

} catch (e) {
    alert(bugCounter + " | " + e.toString() + " | Line: " + e.line)
}