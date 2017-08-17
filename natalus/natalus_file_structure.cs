using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Grasshopper;

namespace natalus
{
    public class natalus_file_structure
    {
        public static string getPathFor(string process)
        {
            string rhinoDocRuntime = Rhino.RhinoDoc.ActiveDoc.RuntimeSerialNumber.ToString();
            string ghPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "Libraries\\Natalus\\NATA\\");

            //Construct filepath for the given process code.
            string finalPath = ghPath + process + "." + rhinoDocRuntime + ".NATA";

            return finalPath;
        }
    }
}
