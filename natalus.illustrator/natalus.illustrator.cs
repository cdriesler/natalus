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
        public static int curve(List<Rhino.Geometry.Curve> geo, List<int> types, int delta, int deltaState)
        {
            //Declare item count vars. TODO: Necessary? Can call .count of discrete lists after sort.
            int line_count = 0;
            int polyline_count = 0;
            int circle_count = 0;

            if (deltaState == 0)
            {
                geo.RemoveRange(0, geo.Count - delta);
            }

            //Sort incoming geometry data into discrete type lists.
            List<Rhino.Geometry.Curve> lineList = new List<Rhino.Geometry.Curve>();
            List<Rhino.Geometry.Polyline> polylineList = new List<Rhino.Geometry.Polyline>();
            List<Rhino.Geometry.Circle> circleList = new List<Rhino.Geometry.Circle>();
            //TODO: Figure out logic for - List<Rhino.Geometry.Polycurve> polycurveList = new List<Rhino.Geometry.Polycurve>();

            //Sorting process. Matches typelist properties to geo list. Tracks where conversion failed if error occurs.
            for (int i = 0; i < geo.Count; i++)
            {
                if (types[i] == 0)
                {
                    lineList.Add(geo[i]);
                    line_count++;
                }
                else if (types[i] == 1)
                {
                    bool polylineBool = geo[i].TryGetPolyline(out Rhino.Geometry.Polyline newPolyline);
                    if (polylineBool == true)
                    {
                        polylineList.Add(newPolyline);
                        polyline_count++;
                    }
                    else if (polylineBool == false)
                    {
                        return 1;
                    }
                }
                else if (types[i] == 2)
                {
                    bool circleBool = geo[i].TryGetCircle(out Rhino.Geometry.Circle newCircle);
                    if (circleBool == true)
                    {
                        circleList.Add(newCircle);
                        circle_count++;
                    }
                    else if (circleBool == false)
                    {
                        return 2;
                    }
                }
            }

            ////Conversion staging utilities. Convert rhino data to nata.
            //Clear staging .nata file and prepare for new data.
            string ghPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "UserObjects\\Natalus\\");
            string docName = RhinoDoc.ActiveDoc.Name;
            string activeSyncStaging = docName.Replace(".3dm", "_staging.nata");
            string activeSyncRuntime = docName.Replace(".3dm", "_runtime.nata");

            string stagingFile = ghPath + activeSyncStaging;

            //Call conversion methods. Handling for empty lists within each method.
            line(deltaState, lineList);
            polyline(deltaState, polylineList);
            circle(deltaState, circleList);

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

            return 0;
        }

        //Line conversion.
        private static void line(int state, List<Rhino.Geometry.Curve> curves)
        {
            //Line conversion logic.
        }
        
        //Linear polyline conversion.
        private static void polyline(int state, List<Rhino.Geometry.Polyline> curves)
        {
            //Linear polyline conversion logic.
        }

        //Circle conversion.
        private static void circle(int state, List<Rhino.Geometry.Circle> curves)
        {
            //Circle converstion logic.
        }

    }
}
