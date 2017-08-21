using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Grasshopper;

using echo;
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

            if (System.IO.File.Exists(D10_Path) == false)
            {
                string docBoxGUID = makeDocBox();

                //Record GUID of docBox.
                System.IO.File.WriteAllText(D10_Path, docBoxGUID);

                return docBoxGUID;
            }
            else if (System.IO.File.Exists(D10_Path) == true)
            {
                //If docBox has already been created, get GUID.
                string docBoxGUID = System.IO.File.ReadAllText(D10_Path);

                return docBoxGUID;
            }

            return "error";
        }

        public static string makeDocBox()
        {
            //If docBox has not been created, create it and place it on its natalus layer.
            /* Abandoning layer idea for now. Not working as intended.
            Rhino.DocObjects.Layer layer_D10 = new Rhino.DocObjects.Layer();
            int layer_D10_index = -1;

            if (RhinoDoc.ActiveDoc.Layers.FindName("D10").Index < 0)
            {
                RhinoDoc.ActiveDoc.Layers.Add(layer_D10);
                layer_D10_index = layer_D10.Index;
            }
            else
            {
                layer_D10 = RhinoDoc.ActiveDoc.Layers.FindName("D10");
                layer_D10_index = layer_D10.Index;
            }
            */

            //Set initial dimensions and record to D01.
            double docBox_width = 12;
            double docBox_height = -12;

            string D01_Path = utils.file_structure.getPathFor("D01");
            System.IO.File.WriteAllText(D01_Path, "12|12");

            Rhino.Geometry.Plane docBox_plane = Rhino.Geometry.Plane.WorldXY;

            Rhino.Geometry.Rectangle3d docBox = new Rhino.Geometry.Rectangle3d(docBox_plane, docBox_width, docBox_height);

            Rhino.DocObjects.ObjectAttributes docBox_attributes = new Rhino.DocObjects.ObjectAttributes();

            //Until layer process resolved, docBox to be on any layer.
            int activeIndex = RhinoDoc.ActiveDoc.Layers.CurrentLayerIndex;
            docBox_attributes.LayerIndex = activeIndex;

            System.Drawing.Color docBoxColor = System.Drawing.Color.LightGray;

            docBox_attributes.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
            docBox_attributes.ObjectColor = docBoxColor;
            docBox_attributes.PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject;
            docBox_attributes.PlotWeight = .8;

            //Determine GUID and to return.
            Guid newGuid = RhinoDoc.ActiveDoc.Objects.AddRectangle(docBox, docBox_attributes);
            string docBoxGUID = newGuid.ToString();

            //Update illustrator boundaries.
            int conversion = utils.units.conversion();
            string jsxPath = utils.file_structure.getJavascriptPath();

            echo.interop echo = new echo.interop();
            echo.docBounds(docBox_width, docBox_height, conversion, jsxPath);

            return docBoxGUID;
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

    public class units
    {
        public static int conversion ()
        {
            return 72;
        }
    }

    public class debug
    {
        public static void ping(int val, string message)
        {
            echo.interop echo = new echo.interop();
            echo.locate(val, message);
        }
    }
}
