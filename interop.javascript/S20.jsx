//alert("Attempting to run S10.jsx");

var selectionChangeData = new File(arguments[0].replace("JSX\\", "NATA\\S21." + arguments[1] + ".NATA"));

var docItems = app.activeDocument.pageItems;

//var debugChangeData = new File("C:\\Users\\Chuck Driesler\\AppData\\Roaming\\Grasshopper\\Libraries\\Natalus\\NATA\\S10.268435458.NATA");
selectionChangeData.open("r");
var data = selectionChangeData.read().split("\n");

//$.writeln(data.0);
var currentSelection = app.selection;

var newSelection = [];

//Iterate through current illustrator selection and test if listed in latest deselections from rhino.
for (i=0, len=currentSelection.length; i < len; i++) {

    check = 0;
    currentName = currentSelection[i].name;

    for (c = 0, len2 = data.length - 1; c < len2; c++) {

        if (currentName == data[c]) {
            check++
        }

    }

    if (check == 0) {
        newSelection.push(currentSelection[i])
    }
}

app.selection = newSelection;

selectionChangeData.close();
