//alert("Attempting to run S10.jsx");

var dataFile = new File(arguments[2] + "S11." + arguments[1] + ".NATA");

//alert(arguments[2] + "S11." + arguments[1] + ".NATA")

var docItems = app.activeDocument.pageItems;

//var debugChangeData = new File("C:\\Users\\Chuck Driesler\\AppData\\Roaming\\Grasshopper\\Libraries\\Natalus\\NATA\\S10.268435458.NATA");
dataFile.open("r");
var data = dataFile.read().split("\n");
dataFile.close();



//TODO: Cull duplicates.

//$.writeln(data.0);

var newSelectedItems = app.selection;

for (i=0, len=data.length - 1; i < len; i++) {
    newSelectedItems.push(docItems.getByName(data[i]));
    }

app.selection = newSelectedItems;
