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
            string rhinoDocRuntime = Rhino.RhinoDoc.ActiveDoc.DocumentId.ToString();
            string docName = "UNSAVED";
            if (Rhino.RhinoDoc.ActiveDoc.Name != null)
            {
                docName = Rhino.RhinoDoc.ActiveDoc.Name.ToUpper().Replace(".3DM", "");
            }

            string docFolder = docName + "." + rhinoDocRuntime;

            string nataPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "Libraries\\Natalus\\NATA\\" + docFolder + "\\");

            System.IO.Directory.CreateDirectory(nataPath);

            //Construct filepath for the given process code.
            string finalPath = nataPath + process + "." + rhinoDocRuntime + ".NATA";

            return finalPath;
        }

        public static string getNataPath()
        {
            string rhinoDocRuntime = Rhino.RhinoDoc.ActiveDoc.DocumentId.ToString();
            string docName = "UNSAVED";
            if (Rhino.RhinoDoc.ActiveDoc.Name != null)
            {
                docName = Rhino.RhinoDoc.ActiveDoc.Name.ToUpper().Replace(".3DM", "");
            }

            string docFolder = docName + "." + rhinoDocRuntime;

            string nataPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "Libraries\\Natalus\\NATA\\" + docFolder + "\\");

            return nataPath;
        }

        public static string getJavascriptPath()
        {
            string jsxPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "Libraries\\Natalus\\JSX\\");

            return jsxPath;
        }

        public static string getDocRuntime()
        {
            string rhinoDocRuntime = Rhino.RhinoDoc.ActiveDoc.DocumentId.ToString();

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

        public static int getGeoDeltaState()
        {
            int geoState = 0;
            string G00_Path = file_structure.getPathFor("G00");
            if (System.IO.File.Exists(G00_Path) == false)
            {
                geoState = 0;
            }
            else
            {
                geoState = Convert.ToInt32(System.IO.File.ReadAllText(G00_Path));
            }

            return geoState;
        }
    }

    public class properties
    {
        public static string tryGetDocBox()
        {
            string D10_Path = file_structure.getPathFor("D10");
            
            if (delta_states.getGeoDeltaState() == 3)
            {
                string docBoxGUID = System.IO.File.ReadAllText(D10_Path);

                return docBoxGUID;
            }
            else
            {
                if (System.IO.File.Exists(D10_Path) == false)
                {
                    string docBoxGUID = makeDocBox();

                    return docBoxGUID;
                }
                else if (System.IO.File.Exists(D10_Path) == true)
                {
                    //If docBox has already been created, get GUID.
                    string docBoxGUID = System.IO.File.ReadAllText(D10_Path);

                    Guid searchGuid = new Guid(docBoxGUID);

                    Rhino.DocObjects.ObjRef docBox = new Rhino.DocObjects.ObjRef(searchGuid);

                    try
                    {
                        string test = docBox.Curve().ToString();

                        return docBoxGUID;
                    }
                    catch (Exception e)
                    {
                        string guid = makeDocBox();

                        return guid;
                    }
                    finally
                    {
                        //?
                    }
                }
            }

            return "error";
        }

        public static string tryGetDocBoxLabel()
        {
            string D30_Path = file_structure.getPathFor("D30");

            string docBoxLabelGUID = System.IO.File.ReadAllText(D30_Path);

            Guid searchGuid = new Guid(docBoxLabelGUID);

            Rhino.DocObjects.ObjRef docBox = new Rhino.DocObjects.ObjRef(searchGuid);

            try
            {
                string test = docBox.TextEntity().ToString();

                return searchGuid.ToString();
            }
            catch (Exception e)
            {
                return "error";
            }
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

            //Set initial dimensions and record to D01 if D01 does not exist.
            string D01_Path = utils.file_structure.getPathFor("D01");
            string D20_Path = utils.file_structure.getPathFor("D20");

            double docBox_width = 12;
            double docBox_height = 12;

            Rhino.Geometry.Point3d refPoint = new Rhino.Geometry.Point3d(0, 0, 0);

            //Check if previous dim configuration existed.
            if (System.IO.File.Exists(D01_Path) == true)
            {
                string[] dims = System.IO.File.ReadAllText(D01_Path).Split('|');

                docBox_width = Convert.ToDouble(dims[0]);
                docBox_height = Convert.ToDouble(dims[1]);
            }
            else
            {
                System.IO.File.WriteAllText(D01_Path, "12|12");
            }

            double adjust = 0;

            if (System.IO.File.Exists(D20_Path) == true)
            {
                string[] coords = System.IO.File.ReadAllText(D20_Path).Split(',');

                refPoint.X = Convert.ToDouble(coords[0]);
                refPoint.Y = Convert.ToDouble(coords[1]);

                adjust = docBox_height;
            }
            else if (System.IO.File.Exists(D20_Path) == false)
            {
                adjust = 0;
            }

            Rhino.Geometry.Plane docBox_plane = Rhino.Geometry.Plane.WorldXY;
            docBox_plane.OriginX = refPoint.X;
            docBox_plane.OriginY = refPoint.Y - adjust;

            Rhino.Geometry.Rectangle3d docBox = new Rhino.Geometry.Rectangle3d(docBox_plane, docBox_width, docBox_height);

            Rhino.DocObjects.ObjectAttributes docBox_attributes = new Rhino.DocObjects.ObjectAttributes();

            //Until layer process resolved, docBox to be on any layer.
            int activeIndex = RhinoDoc.ActiveDoc.Layers.CurrentLayerIndex;
            docBox_attributes.LayerIndex = activeIndex;

            //(Rhino 5) Convert docBox Rectangle3D to polyline curve.
            Rhino.Geometry.Polyline docBoxPolyline = docBox.ToPolyline();

            //Freeze updating while docBox is created.
            string x10_path = utils.file_structure.getPathFor("x10");
            System.IO.File.WriteAllText(x10_path, "false");

            //Determine GUID and record to D10.
            Guid newGuid = RhinoDoc.ActiveDoc.Objects.AddPolyline(docBoxPolyline);

            Rhino.DocObjects.ObjRef docBoxObj = new Rhino.DocObjects.ObjRef(newGuid);
            Rhino.DocObjects.CurveObject docBoxCurve = docBoxObj.Object() as Rhino.DocObjects.CurveObject;

            //Set curve to Illustrator orange.
            System.Drawing.Color docBoxColor = System.Drawing.Color.FromArgb(240, 120, 6);

            docBoxCurve.Attributes.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
            docBoxCurve.Attributes.ObjectColor = docBoxColor;
            docBoxCurve.Attributes.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject;
            docBoxCurve.Attributes.PlotColor = docBoxColor;
            docBoxCurve.Attributes.PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject;
            docBoxCurve.Attributes.PlotWeight = 1.5;

            docBoxCurve.CommitChanges();

            //Label it!
            Rhino.Geometry.TextEntity label = new Rhino.Geometry.TextEntity();

            label.TextHeight = .4;

            Rhino.Geometry.Plane label_plane = Rhino.Geometry.Plane.WorldXY;
            label_plane.OriginX = docBox_plane.OriginX;
            label_plane.OriginY = docBox_plane.OriginY - .4 - .1;
            label.Plane = label_plane;

            label.Text = ("Linked Illustrator Artboard");

            Guid docBoxLabel = RhinoDoc.ActiveDoc.Objects.AddText(label);

            Rhino.DocObjects.ObjRef labelObj = new Rhino.DocObjects.ObjRef(docBoxLabel);
            Rhino.DocObjects.TextObject labelText = labelObj.Object() as Rhino.DocObjects.TextObject;

            labelText.Attributes.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
            labelText.Attributes.ObjectColor = docBoxColor;
            labelText.Attributes.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject;
            labelText.Attributes.PlotColor = docBoxColor;

            labelText.CommitChanges();

            string docBoxGUID = newGuid.ToString();

            string D10_Path = utils.file_structure.getPathFor("D10");
            if (System.IO.File.Exists(D10_Path) && System.IO.File.ReadAllText(D10_Path) != "")
            {
                string D11_Path = utils.file_structure.getPathFor("D11");
                System.IO.File.WriteAllText(D11_Path, System.IO.File.ReadAllText(D10_Path));
            }
            System.IO.File.WriteAllText(D10_Path, docBoxGUID);

            string docBoxLabelGUID = docBoxLabel.ToString();

            string D30_Path = utils.file_structure.getPathFor("D30");
            if (System.IO.File.Exists(D30_Path) && System.IO.File.ReadAllText(D30_Path) != "")
            {
                string D31_Path = utils.file_structure.getPathFor("D31");
                System.IO.File.WriteAllText(D31_Path, System.IO.File.ReadAllText(D30_Path));
            }
            System.IO.File.WriteAllText(D30_Path, docBoxGUID);

            //Unfreeze updating.
            System.IO.File.WriteAllText(x10_path, "True");

            //Update illustrator boundaries.
            int conversion = utils.units.conversion();
            string jsxPath = utils.file_structure.getJavascriptPath();

            echo.interop echo = new echo.interop();
            echo.docBounds(docBox_width, System.Math.Abs(docBox_height), conversion, jsxPath);

            return docBoxGUID;
        }

        public static Rhino.Geometry.Point3d getRefPoint()
        {
            Guid docBoxID = new Guid(utils.properties.tryGetDocBox());
            Rhino.DocObjects.ObjRef docBox = new Rhino.DocObjects.ObjRef(docBoxID);

            Rhino.Geometry.Curve docBoxCurve = docBox.Curve();

            List<double> xVals = new List<double>();
            List<double> yVals = new List<double>();

            int spanCount = docBoxCurve.SpanCount;
            for (int i = 0; i < spanCount; i++)
            {
                double activeParameter = docBoxCurve.SpanDomain(i).Min;
                Rhino.Geometry.Point3d activePoint = docBoxCurve.PointAt(activeParameter);

                double activeX = activePoint.X;
                xVals.Add(activeX);

                double activeY = activePoint.Y;
                yVals.Add(activeY);
            }

            xVals.Sort();
            yVals.Sort();

            Rhino.Geometry.Point3d docBoxRefPoint = new Rhino.Geometry.Point3d(xVals[0], yVals[spanCount - 1], 0);

            return docBoxRefPoint;
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

        public static void alert(string message)
        {
            echo.interop echo = new echo.interop();
            echo.alert(message);
        }
    }
}
