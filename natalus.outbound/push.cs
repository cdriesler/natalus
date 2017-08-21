using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Grasshopper;

using echo;
using natalus.utils;

namespace natalus.outbound
{
    public class push
    {
        //Write GUIDs of recently selected or undeleted objects to .NATA file as event fires.
        public static int selectionIncrease(Rhino.DocObjects.RhinoObject[] newSelectedObjects)
        {
            //Determine filepath for active processes.
            string statePath = utils.file_structure.getPathFor("S00");           //S00 - Record state of change.
            string destinationPath = utils.file_structure.getPathFor("S10");    //S10 - Increasing selection, write GUIDs.

            //Debug util: ping if event fired.
            //int c = newSelectedObjects.Length;
            //
            //echo.interop debug = new echo.interop();
            //debug.locate(c, destinationPath);

            //Write GUIDs to file.
            for (int i = 0; i < newSelectedObjects.Length; i++)
            {
                string newSelectedGUID = newSelectedObjects[i].Id.ToString();
                System.IO.File.AppendAllText(destinationPath, newSelectedGUID + Environment.NewLine);
            }

            //Set state file S00 to 1: selection increasing.
            System.IO.File.WriteAllText(statePath, "1");

            return 0;
        }

        //Write GUIDs of recently deselected objects to .NATA file as event fires.
        public static int selectionDecrease(Rhino.DocObjects.RhinoObject[] newDeselectedObjects)
        {
            //Determine filepath for active processes.
            string statePath = utils.file_structure.getPathFor("S00");           //S00 - Record state of change.
            string destinationPath = utils.file_structure.getPathFor("S20");    //S20 - Decreasing selection, write GUIDs.

            //Write GUIDs to file.
            for (int i = 0; i < newDeselectedObjects.Length; i++)
            {
                string newSelectedGUID = newDeselectedObjects[i].Id.ToString();
                System.IO.File.AppendAllText(destinationPath, newSelectedGUID + Environment.NewLine);
            }

            //Set state file S00 to 2: selection decreasing.
            System.IO.File.WriteAllText(statePath, "2");

            return 1;
        }

        //Streamlined deselection process triggered on a full deselection or object deletion.
        public static int selectionReset()
        {
            //Set state file S00 to 3: selection reset.
            string statePath = utils.file_structure.getPathFor("S00");
            System.IO.File.WriteAllText(statePath, "3");

            string jsxPath = utils.file_structure.getJavascriptPath();
            string runtime = utils.file_structure.getDocRuntime();

            echo.interop echo = new echo.interop();
            echo.selection(3, jsxPath, runtime);

            return 2;
        }

        //On RhinoDoc.Idle, push previously recorded changes in selection to Illustrator. 
        //Clear all S series .nata files when Illustrator finishes.
        public static void selectionToIllustrator(int state)
        {
            //Reset S00 so that RhinoApp.Idle doesn't repeat this process.
            string statePath = utils.file_structure.getPathFor("S00");
            System.IO.File.WriteAllText(statePath, "0");
            //int state = Convert.ToInt32(System.IO.File.ReadAllText(statePath));

            //Debug util: record state currently being passed as an argument.
            string stateDebugPath = utils.file_structure.getPathFor("x00");
            File.WriteAllText(stateDebugPath, "Current delta state: " + state.ToString());

            //Determine filepath for illustrator extendscript processes.
            string jsxPath = utils.file_structure.getJavascriptPath();

            //Determine current document runtime to send to interop.
            string runtime = utils.file_structure.getDocRuntime();

            //Generate copies of current data that Illustrator will read.
            string S10_Path = utils.file_structure.getPathFor("S10");
            string S20_Path = utils.file_structure.getPathFor("S20");

            if (File.Exists(S10_Path) == false | File.ReadAllText(S10_Path) == "")
            {
                File.WriteAllText(S10_Path, "empty");
            }
            if (File.Exists(S20_Path) == false | File.ReadAllText(S20_Path) == "")
            {
                File.WriteAllText(S20_Path, "empty");
            }

            string S11_Path = utils.file_structure.getPathFor("S11");
            string S21_Path = utils.file_structure.getPathFor("S21");

            File.WriteAllText(S11_Path, File.ReadAllText(S10_Path));
            File.WriteAllText(S21_Path, File.ReadAllText(S20_Path));

            //Tell illustrator to run selection update script, based on state.
            echo.interop echo = new echo.interop();
            echo.selection(state, jsxPath, runtime);

            //Clear original .nata file data.
            clearSelectionNata();
        }

        //Clear selection .nata files on successful illustrator sync. Raise error if failure ocurred.
        private static void clearSelectionNata()
        {
            //Determine paths for all selection .nata files.
            string S10_Path = utils.file_structure.getPathFor("S10");
            string S20_Path = utils.file_structure.getPathFor("S20");
            string S01_Path = utils.file_structure.getPathFor("S01");

            //PURGE ALL TEXT
            System.IO.File.WriteAllText(S10_Path, "");
            System.IO.File.WriteAllText(S20_Path, "");
            System.IO.File.WriteAllText(S01_Path, "false");
        }
    }
}
