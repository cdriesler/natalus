//alert("Attempting to run G20...");

//Passed arguments:
//0 - Path to javascript directory.
//1 - Current runtime id.
//2 - Path to document NATA.
//3 - conversion ratio

var doc = app.activeDocument;

var dataFile = new File(arguments[2] + "G21." + arguments[1] + ".NATA");

var tolerance = 5;
var conversion = arguments[3];

dataFile.open("r");

var data = dataFile.read().split("\n");

dataFile.close();

if (data[0] == "empty") {
    //Do nothing.
}
else
{
    for (i = 0, len = data.length - 1; i < len; i++)
    {
        doc.pageItems.getByName(data[i]).remove();
    }
}