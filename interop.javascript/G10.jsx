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

if (data[0] == "empty")
{
    //Do nothing.
}
else
{

//alert("Made it here: " + data[0]);

//Geometry NATA structure:
//0 - Type
//  0 - Linear curve
//  1 - Linear polyline
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
                var points = dataset[4].split(",");

                var x1 = points[0].substring(0, tolerance) * conversion;
                var y1 = points[1].substring(0, tolerance) * conversion;
                var x2 = points[2].substring(0, tolerance) * conversion;
                var y2 = points[3].substring(0, tolerance) * conversion;

                var newPath = doc.pathItems.add();

                newPath.stroked = true;

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

                app.redraw();

                newPath.name = guid;
                newPath.move(activeLayer, ElementPlacement.PLACEATEND);
            }

            if (type == 1) {
                for (c = 0, len2 = spans; c < len2; c++) {

                }
            }

        } //finally


    /* Previous reference:
    var pathName =  data[0];
var pathLayer = data[1];
//TODO: store points within their own array
var points = data[2].split(",");
var x1 = points[0].substring(0,5) * conv;
var y1 = points[1].substring(0,5) * conv;
var x2 = points[2].substring(0,5) * conv;
var y2 = points[3].substring(0,5) * conv;

var myLine = doc.pathItems.add();
//set stroked to true so we can see the path
myLine.stroked = true;
var newPoint = myLine.pathPoints.add();
newPoint.anchor = [x1,y1];
//bugCatcher++;

//giving the direction points the same value as the
//anchor point creates a straight line segment
newPoint.leftDirection = newPoint.anchor;
newPoint.rightDirection = newPoint.anchor;
newPoint.pointType = PointType.CORNER;
var newPoint1 = myLine.pathPoints.add();
newPoint1.anchor = [x2,y2];
newPoint1.leftDirection = newPoint1.anchor;
newPoint1.rightDirection = newPoint1.anchor;
newPoint1.pointType = PointType.CORNER;


app.redraw();

myLine.name = arguments[1];
    */

    }

}//else