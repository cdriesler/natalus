//alert("Attempting to run S10.jsx");

var selectionChangeData = new File(arguments[0].replace("JSX\\", "NATA\\S11." + arguments[1] + ".NATA"));

var docItems = app.activeDocument.pageItems;

//var debugChangeData = new File("C:\\Users\\Chuck Driesler\\AppData\\Roaming\\Grasshopper\\Libraries\\Natalus\\NATA\\S10.268435458.NATA");
selectionChangeData.open("r");
var data = selectionChangeData.read().split("\n");

//TODO: Cull duplicates.

//$.writeln(data.0);

var newSelectedItems = app.selection;

for (i=0, len=data.length - 1; i < len; i++) {
    newSelectedItems.push(docItems.getByName(data[i]));
    }

app.selection = newSelectedItems;

selectionChangeData.close();
