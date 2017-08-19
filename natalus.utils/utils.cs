using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Grasshopper;

using natalus.inbound;

namespace natalus.utils
{
    public class file_structure
    {
        public static string getPathFor(string process)
        {
            string rhinoDocRuntime = Rhino.RhinoDoc.ActiveDoc.RuntimeSerialNumber.ToString();
            string nataPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "Libraries\\Natalus\\NATA\\");

            System.IO.Directory.CreateDirectory(nataPath);

            //Construct filepath for the given process code.
            string finalPath = nataPath + process + "." + rhinoDocRuntime + ".NATA";

            return finalPath;
        }

        public static string getJavascriptPath()
        {
            string jsxPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "Libraries\\Natalus\\JSX\\");

            return jsxPath;
        }

        public static string getDocRuntime()
        {
            string rhinoDocRuntime = Rhino.RhinoDoc.ActiveDoc.RuntimeSerialNumber.ToString();

            return rhinoDocRuntime;
        }
    }

    public class delta_states
    {
        public static int getSelectionState()
        {
            string S00_Path = file_structure.getPathFor("S00");
            int selectionState = Convert.ToInt32(System.IO.File.ReadAllText(S00_Path));

            return selectionState;
        }
    }

    public class properties
    {
        public static string getDocBoxID()
        {
            string D10_Path = file_structure.getPathFor("D10");

            /*
            bool checkA = System.IO.File.Exists(D10_Path);

            bool checkB = false;
            Rhino.RhinoDoc.ActiveDoc.Layers docBoxLayer = RhinoDoc.ActiveDoc.Layers.FindName("D10");

            if (docBoxLayer)
            {
                checkB = true;
            } */

            if (System.IO.File.Exists(D10_Path) == false)
            {
                return "missing";
            }
            else if (System.IO.File.Exists(D10_Path) == true)
            {
                //If docBox has already been created, a D10 will exist or
                string docBoxGUID = System.IO.File.ReadAllText(D10_Path);

                return docBoxGUID;
            }

            return "error";
        }

        //Bad workaround attempt. Event handler interaction with grasshopper input not working like I want it to.
        public static bool getPushState()
        {
            string path = file_structure.getPathFor("x10");
            bool state = Convert.ToBoolean(System.IO.File.ReadAllText(path));

            return state;
        }

        public static void setPushState(bool newState)
        {
            string path = file_structure.getPathFor("x10");
            System.IO.File.WriteAllText(path, newState.ToString());
        }
    }
}
