using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Grasshopper;

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
}
