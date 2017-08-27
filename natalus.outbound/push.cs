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

            //Cache docbox GUID.
            string docBoxID = utils.properties.tryGetDocBox();
            string D11_Path = utils.file_structure.getPathFor("D11");
            string pastDocBox = "";
            if (System.IO.File.Exists(D11_Path))
            {
                pastDocBox = System.IO.File.ReadAllText(D11_Path);
            }

            //Write GUIDs to file.
            for (int i = 0; i < newSelectedObjects.Length; i++)
            {
                string newSelectedGUID = newSelectedObjects[i].Id.ToString();
                if (newSelectedGUID == docBoxID | newSelectedGUID == pastDocBox)
                {
                    //Skip it. It's the docBox and is not represented in illustrator.
                }
                else
                {
                    if (newSelectedObjects[i].ObjectType.ToString().ToLower().Contains("curve") == true)
                    {
                        System.IO.File.AppendAllText(destinationPath, newSelectedGUID + Environment.NewLine);
                    }
                }
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

            //Cache docbox GUID.
            string docBoxID = utils.properties.tryGetDocBox();
            string D11_Path = utils.file_structure.getPathFor("D11");
            string pastDocBox = "";
            if (System.IO.File.Exists(D11_Path))
            {
                pastDocBox = System.IO.File.ReadAllText(D11_Path);
            }

            //Write GUIDs to file.
            for (int i = 0; i < newDeselectedObjects.Length; i++)
            {
                string newSelectedGUID = newDeselectedObjects[i].Id.ToString();
                if (newSelectedGUID == docBoxID | newSelectedGUID == pastDocBox)
                {
                    //Skip it. It's the docBox and is not represented in illustrator.
                }
                else
                {
                    if (newDeselectedObjects[i].ObjectType.ToString().ToLower().Contains("curve") == true)
                    {
                        System.IO.File.AppendAllText(destinationPath, newSelectedGUID + Environment.NewLine);
                    }
                }
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
            string nataPath = utils.file_structure.getNataPath();
            int conversion = utils.units.conversion();

            echo.interop echo = new echo.interop();
            echo.selection(3, jsxPath, runtime, nataPath, conversion);

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
            File.WriteAllText(stateDebugPath, "Current selection delta state: " + state.ToString());

            //Determine filepath for illustrator extendscript processes.
            string jsxPath = utils.file_structure.getJavascriptPath();

            //Determine current document runtime to send to interop.
            string runtime = utils.file_structure.getDocRuntime();

            //Generate copies of current data that Illustrator will read.
            string S10_Path = utils.file_structure.getPathFor("S10");
            string S20_Path = utils.file_structure.getPathFor("S20");

            if (File.Exists(S10_Path) == false)
            {
                File.WriteAllText(S10_Path, "empty");
            }
            else if (File.Exists(S10_Path) == true && File.ReadAllText(S10_Path) == "")
            {
                File.WriteAllText(S10_Path, "empty");
            }
            if (File.Exists(S20_Path) == false)
            {
                File.WriteAllText(S20_Path, "empty");
            }
            else if (File.Exists(S20_Path) == true && File.ReadAllText(S20_Path) == "")
            {
                File.WriteAllText(S20_Path, "empty");
            }

            string S11_Path = utils.file_structure.getPathFor("S11");
            string S21_Path = utils.file_structure.getPathFor("S21");

            File.WriteAllText(S11_Path, File.ReadAllText(S10_Path));
            File.WriteAllText(S21_Path, File.ReadAllText(S20_Path));

            string nataPath = utils.file_structure.getNataPath();
            int conversion = utils.units.conversion();

            //Clear original .nata file data.
            clearSelectionNata();

            //Tell illustrator to run selection update script, based on state.
            echo.interop echo = new echo.interop();
            echo.selection(state, jsxPath, runtime, nataPath, conversion);
        }

        //Clear selection .nata files on successful illustrator sync. Raise error if failure ocurred.
        public static void clearSelectionNata()
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

        //Update illustrator doc bound if docbox changes in rhino.
        public static void docBoxChanges(Rhino.DocObjects.RhinoObject docBox)
        {
            //Parse rectangle from incoming RhinoObject.
            Guid incomingGuid = docBox.Id;
            Rhino.Geometry.Curve newCurve = null;

            Rhino.DocObjects.ObjRef oRef = new Rhino.DocObjects.ObjRef(incomingGuid);

            newCurve = oRef.Curve();

            Rhino.Geometry.BoundingBox newDimsBox = newCurve.GetBoundingBox(false);

            Rhino.Geometry.Point3d newMinPoint = newDimsBox.Min;
            Rhino.Geometry.Point3d newMaxPoint = newDimsBox.Max;

            double newWidth = newMaxPoint.X - newMinPoint.X;
            double newHeight = newMaxPoint.Y - newMinPoint.Y;

            int conversion = utils.units.conversion();

            //Compare previous size, or record if first run.
            string D01_Path = utils.file_structure.getPathFor("D01");
            string jsxPath = utils.file_structure.getJavascriptPath();

            if (System.IO.File.Exists(D01_Path) == false)
            {
                string newData = newWidth.ToString() + "|" + newHeight.ToString();
                System.IO.File.WriteAllText(D01_Path, newData);
            }
            else if (System.IO.File.Exists(D01_Path) == true)
            {
                //If file exists, verify bounds changed. (Otherwise, box moved.)
                string[] oldData = System.IO.File.ReadAllText(D01_Path).Split('|');
                double oldWidth = Convert.ToDouble(oldData[0]);
                double oldHeight = Convert.ToDouble(oldData[1]);

                //utils.debug.ping(0, "Width change: " + newWidth.ToString() + " - " + oldWidth.ToString());

                if (oldWidth == newWidth && oldHeight == newHeight)
                {
                    //outbound.push.fullReset();
                }
                else
                {
                    string newData = newWidth.ToString() + "|" + newHeight.ToString();
                    System.IO.File.WriteAllText(D01_Path, newData);

                    echo.interop echo = new echo.interop();
                    echo.docBounds(newWidth, newHeight, conversion, jsxPath);

                    //outbound.push.fullReset();
                }
                
            }
        }

        //Reset all synced illustrator lines if docbounds change.
        private static void fullReset()
        {
            //Is there any chance this could be fast?
        }

        //Update geometry in illustrator.
        public static void geometryToIllustrator(int state)
        {
            //Reset G00 so that RhinoApp.Idle doesn't repeat this process.
            string statePath = utils.file_structure.getPathFor("G00");
            System.IO.File.WriteAllText(statePath, "0");

            //Debug util: record previous state.
            string debugPath = utils.file_structure.getPathFor("x20");
            System.IO.File.WriteAllText(debugPath, "Previous delta state: " + state.ToString());

            //Determine filepath for illustrator extendscript processes.
            string jsxPath = utils.file_structure.getJavascriptPath();
            string nataPath = utils.file_structure.getNataPath();

            //Determine current document runtime to send to interop.
            string runtime = utils.file_structure.getDocRuntime();

            //Generate copies of current data that Illustrator will read.
            string G10_Path = utils.file_structure.getPathFor("G10");
            string G20_Path = utils.file_structure.getPathFor("G20");

            if (File.Exists(G10_Path) == false)
            {
                //System.Threading.Thread.Sleep(500); //Weird things happening with initial file creation...
                File.WriteAllText(G10_Path, "empty");
            }
            else if (File.Exists(G10_Path) == true && File.ReadAllText(G10_Path) == "")
            {
                File.WriteAllText(G10_Path, "empty");
            }
            if (File.Exists(G20_Path) == false)
            {
                File.WriteAllText(G20_Path, "empty");
            }
            else if (File.Exists(G20_Path) == true && File.ReadAllText(G20_Path) == "")
            {
                File.WriteAllText(G20_Path, "empty");
            }

            string G11_Path = utils.file_structure.getPathFor("G11");
            string G21_Path = utils.file_structure.getPathFor("G21");

            File.WriteAllText(G11_Path, File.ReadAllText(G10_Path));
            File.WriteAllText(G21_Path, File.ReadAllText(G20_Path));

            int conversion = utils.units.conversion();

            //Clear original .nata file data.
            clearGeometryNata();

            //Tell illustrator to run geometry update script, based on state.
            echo.interop echo = new echo.interop();
            echo.geometry(state, jsxPath, runtime, nataPath, conversion);
        }

        public static void clearGeometryNata()
        {
            //debug.alert("Clearing Geometery Nata");

            //Determine paths for all selection .nata files.
            string G10_Path = utils.file_structure.getPathFor("G10");
            string G20_Path = utils.file_structure.getPathFor("G20");

            //PURGE ALL TEXT
            System.IO.File.WriteAllText(G10_Path, "");
            System.IO.File.WriteAllText(G20_Path, "");

            //debug.alert("Geometry Nata successfully cleared");
        }
    }
}
