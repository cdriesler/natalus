using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;

using echo;

namespace natalus.illustrator
{
    public class nata_ai
    {
        //Determine what geometry needs to be changed in Illustrator. Update .nata file accordingly, push indexes to Illustrator.
        public static string curve(List<Rhino.Geometry.Curve> geo, List<int> types, int delta, int deltaState, uint prev_runtime)
        {
            //Declare item count vars. TODO: Necessary? Can call .count of discrete lists after sort.
            int line_count = 0;
            int polyline_count = 0;
            int circle_count = 0;

            //Create list for Rhino.DocObjects.RhinoObject . Necessary to get GUIDs and other object data not in Rhino.Geometry.
            List<Rhino.DocObjects.RhinoObject> newestObjects = new List<Rhino.DocObjects.RhinoObject>();

            if (deltaState == 0)
            {
                //Reduce geo list to new items only, if deltaState says items are being added.
                geo.RemoveRange(0, geo.Count - delta);

                //Gather newest geometry, pass to newestObjects list.
                Rhino.DocObjects.RhinoObject[] newestObjs = Rhino.RhinoDoc.ActiveDoc.Objects.AllObjectsSince(prev_runtime);
                for (int i = 0; i < newestObjs.Length; i++)
                {
                    if (newestObjs[i].GetType().ToString().Contains("Curve")) //Enum value is 4.
                    {
                        newestObjects.Add(newestObjs[i]);
                    }
                }      
            }

            //Generate GUID list from newestObjects list.
            //TODO: This seems sloppy/backwards. Totally parallel to the geometry sorting, even though I can get the geometry from the RhinoObject.CurveObject class.
            List<string> guidList = new List<string>();

            for (int i = 0; i < newestObjects.Count; i++)
            {
                guidList.Add(newestObjects[i].Id.ToString());
            }

            //Declare NATA filepath.
            string ghPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "UserObjects\\Natalus\\NATA\\");
            string docName = RhinoDoc.ActiveDoc.Name;

            //Debug util: record length of geo set and current deltaState. List new GUIDS.
            string activeSyncNewCount = docName.Replace(".3dm", "_newcount.nata");
            System.IO.File.WriteAllText(ghPath + activeSyncNewCount, geo.Count.ToString() + " (geo count)|" + deltaState.ToString() + " (delta state)|" + guidList.Count.ToString() + " (guid count)");
            for (int i = 0; i < guidList.Count; i++)
            {
                System.IO.File.AppendAllText(ghPath + activeSyncNewCount, guidList[i]);
            }

            //Sort incoming geometry data into discrete type lists.
            List<Rhino.Geometry.Curve> lineList = new List<Rhino.Geometry.Curve>();
            List<Rhino.Geometry.Polyline> polylineList = new List<Rhino.Geometry.Polyline>();
            List<Rhino.Geometry.Circle> circleList = new List<Rhino.Geometry.Circle>();
            //TODO: Figure out logic for - List<Rhino.Geometry.Polycurve> polycurveList = new List<Rhino.Geometry.Polycurve>();

            //Create parallel index list.
            List<int> guidReferenceList = new List<int>();

            //Sorting process. Matches typelist properties to geo list. Tracks where conversion failed if error occurs.
            for (int i = 0; i < geo.Count; i++)
            {
                if (types[i] == 0)
                {
                    lineList.Add(geo[i]);
                    line_count++;

                    guidReferenceList.Add(i);
                }
                else if (types[i] == 1)
                {
                    bool polylineBool = geo[i].TryGetPolyline(out Rhino.Geometry.Polyline newPolyline);
                    if (polylineBool == true)
                    {
                        polylineList.Add(newPolyline);
                        polyline_count++;

                        guidReferenceList.Add(i);
                    }
                    else if (polylineBool == false)
                    {
                        string errorType = geo[i].ObjectType.ToString();
                        return "Error Code 1: " + errorType + " in polyline routine.";
                    }
                }
                else if (types[i] == 2)
                {
                    bool circleBool = geo[i].TryGetCircle(out Rhino.Geometry.Circle newCircle);
                    if (circleBool == true)
                    {
                        circleList.Add(newCircle);
                        circle_count++;

                        guidReferenceList.Add(i);
                    }
                    else if (circleBool == false)
                    {
                        string errorType = geo[i].ObjectType.ToString();
                        return "Error Code 2: " + errorType + " in circle routine.";
                    }
                }
            }

            ////Conversion staging utilities. Convert rhino data to nata.
            //Clear staging .nata file and prepare for new data.
            //-string ghPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "UserObjects\\Natalus\\NATA\\");
            //-string docName = RhinoDoc.ActiveDoc.Name;
            string activeSyncStaging = docName.Replace(".3dm", "_staging.nata");
            string activeSyncRuntime = docName.Replace(".3dm", "_runtime.nata");

            string stagingFile = ghPath + activeSyncStaging;

            int rollingIndex = 0;

            //Call conversion methods. Handling for empty lists within each method.
            int increment = line(deltaState, lineList, rollingIndex, guidReferenceList, guidList);
            rollingIndex = rollingIndex + increment;

            increment = polyline(deltaState, polylineList, rollingIndex, guidReferenceList, guidList);
            rollingIndex = rollingIndex + increment;

            circle(deltaState, circleList, rollingIndex, guidReferenceList, guidList);

            //Pass current change state to Illustrator and ask it to update.
            echo.interop echo = new interop();
            echo.illustrator(deltaState, stagingFile);

            //Record runtime serialization number of most recent object to facilitate tracking next time file updates.
            uint latestRuntime = RhinoDoc.ActiveDoc.Objects.MostRecentObject().RuntimeSerialNumber;

            System.IO.File.WriteAllText(ghPath + activeSyncRuntime, latestRuntime.ToString());

            ////Debug: declare change.
            //string debugMessage = count.ToString() + " - " + prevCount.ToString() +  " | Change is: " + delta.ToString();
            //
            //echo.interop echo = new interop();
            //echo.locate(0, debugMessage);

            return "0";
        }

        //Line conversion.
        private static int line(int state, List<Rhino.Geometry.Curve> curves, int rIndex, List<int> gRefIndex, List<string> guid)
        {
            string ghPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "UserObjects\\Natalus\\NATA\\");
            string docName = RhinoDoc.ActiveDoc.Name;
            string activeSyncStaging = docName.Replace(".3dm", "_staging.nata");
            string activeSyncRuntime = docName.Replace(".3dm", "_runtime.nata");

            string stagingFile = ghPath + activeSyncStaging;

            //Additive line conversion logic.
            if (state == 0)
            {
                for (int i = 0; i < curves.Count; i++)
                {
                    string id = guid[gRefIndex[rIndex]];

                    //Organize data into .nata structure.
                    //"delta state" | "geo type" | "guid" | "geometric information"
                    string lineData = state.ToString() + "|" + "0" + "|" + id + "|" + "Coordinates Placeholder" + Environment.NewLine;

                    //Add information to staging file.
                    System.IO.File.AppendAllText(stagingFile, lineData);

                    rIndex++;
                }

                echo.interop echo = new echo.interop();

                echo.locate(0, "Additive line process iterated " + rIndex.ToString() + " times.");

                return rIndex;
            }
            //Subtractve line conversion logic.
            if (state == 1)
            {
                //TODO
                return rIndex;
            }
            //Tranformative line conversion logic.
            if (state == 2)
            {
                //TODO
                return rIndex;
            }
            return rIndex;
        }
        
        //Linear polyline conversion.
        private static int polyline(int state, List<Rhino.Geometry.Polyline> curves, int rIndex, List<int> gRefIndex, List<string> guid)
        {
            //Linear polyline conversion logic.
        }

        //Circle conversion.
        private static void circle(int state, List<Rhino.Geometry.Circle> curves, int rIndex, List<int> gRefIndex, List<string> guid)
        {
            //Circle converstion logic.
        }

    }
}
