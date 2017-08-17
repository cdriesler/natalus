using System;
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

            //Write GUIDs to file.
            for (int i = 0; i < newSelectedObjects.Length; i++)
            {
                string newSelectedGUID = newSelectedObjects[i].Id.ToString();
                System.IO.File.AppendAllText(destinationPath, newSelectedGUID + Environment.NewLine);
            }

            //Set state file S00 to 0: selection increasing.
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

            //Set state file S00 to 0: selection increasing.
            System.IO.File.WriteAllText(statePath, "2");

            return 1;
        }

        //Streamlined deselection process triggered on a full deselection or object deletion.
        public static int selectionReset()
        {
            //Set state file S00 to 2: selection reset.
            string statePath = utils.file_structure.getPathFor("S00");
            System.IO.File.WriteAllText(statePath, "3");

            return 2;
        }

        //On RhinoDoc.Idle, push previously recorded changes in selection to Illustrator. 
        //Clear all S series .nata files when Illustrator finishes.
        public static void selectionToIllustrator()
        {
            //Determine state of selection changes that just ocurred from .nata file S00.
            string statePath = utils.file_structure.getPathFor("S00");
            int state = Convert.ToInt32(System.IO.File.ReadAllText(statePath));

            //Determine filepath for illustrator extendscript processes.
            string jsxPath = utils.file_structure.getJavascriptPath();

            //Create watchfile for Illustrator's confirmation state file.
            string watchFilePath = utils.file_structure.getPathFor("S01");   //S01 - State file written by illustrator.
            System.IO.FileSystemWatcher fw = new System.IO.FileSystemWatcher(watchFilePath);
            fw.EnableRaisingEvents = true;
            fw.Changed += (sender, e) => clearSelectionNata();

            //Tell illustrator to run selection update script, based on state.
            echo.interop echo = new echo.interop();
            echo.selection(state, jsxPath);
        }

        //Clear selection .nata files on successful illustrator sync. Raise error if failure ocurred.
        private static void clearSelectionNata()
        {
            //Determine paths for all selection .nata files.
            string S00_Path = utils.file_structure.getPathFor("S00");
            string S10_Path = utils.file_structure.getPathFor("S10");
            string S20_Path = utils.file_structure.getPathFor("S20");
            string S01_Path = utils.file_structure.getPathFor("S01");

            bool successBool = Convert.ToBoolean(System.IO.File.ReadAllText(S01_Path));

            //Purge all text, making sure to change S01 last. When the fw.Changed event fires again, it will read false and stop the loop.
            if (successBool == true)
            {
                System.IO.File.WriteAllText(S00_Path, "0");
                System.IO.File.WriteAllText(S10_Path, "");
                System.IO.File.WriteAllText(S20_Path, "");
                System.IO.File.WriteAllText(S01_Path, "");
            }
        }
    }
}
