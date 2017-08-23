using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natalus.outbound
{
    public class translate
    {
        public static void curves(int state, Rhino.DocObjects.RhinoObject geo)
        {
            //Verify geometery is being added, not transformed.
            string G00_Path = utils.file_structure.getPathFor("G00");
            int verifyState = Convert.ToInt32(System.IO.File.ReadAllText(G00_Path));

            if (verifyState == 3)
            {
                state = 3;
            }
            else
            {
                System.IO.File.WriteAllText(G00_Path, state.ToString());
            }

            //Grab reference point from docBox.
            Rhino.Geometry.Point3d refPoint = utils.properties.getRefPoint();
            double refPointX = refPoint.X;
            double refPointY = refPoint.Y;

            //Determine and sanitize cache file for new geometry.
            string G10_Path = utils.file_structure.getPathFor("G10");

            if (System.IO.File.Exists(G10_Path) == false)
            {
                System.IO.File.WriteAllText(G10_Path, "");
                //System.Threading.Thread.Sleep(500);
            }
            
            //Parse curve geometry from RhinoObject.
            Guid incomingGuid = geo.Id;
            Rhino.Geometry.Curve newCurve = null;

            int incomingLayerIndex = geo.Attributes.LayerIndex;
            Rhino.DocObjects.Layer incomingLayer = Rhino.RhinoDoc.ActiveDoc.Layers.CurrentLayer;
            string incomingLayerName = incomingLayer.Name;

            Rhino.DocObjects.ObjRef oRef = new Rhino.DocObjects.ObjRef(incomingGuid);

            //Verify object is a curve, otherwise quit.
            if (oRef.Geometry().ObjectType.ToString().ToLower().Contains("curve"))
            {
                newCurve = oRef.Curve();
            }
            else
            {
                //System.IO.File.AppendAllText(G10_Path, "X|" + Environment.NewLine);
                return;
            }

            //Run curve geometry through the coin sorter.
            //All data is cached in the same file, but keyed at the beginning by type.
            //0 - linear curve
            //1 - linear polyline
            //2 - arc
            //3 - circle
            //4 - ellipse
            //5 - arbitrary curve
            //6 - complex polycurve
            if (newCurve.Degree == 1)
            {
                if (newCurve.SpanCount == 1)
                {
                    //Treat as linear curve.
                    //utils.debug.ping(0, "Linear curve added!");

                    Rhino.Geometry.Point3d startPoint = newCurve.PointAtStart;
                    Rhino.Geometry.Point3d endPoint = newCurve.PointAtEnd;

                    string x1 = (startPoint.X - refPointX).ToString();
                    string y1 = (startPoint.Y - refPointY).ToString();
                    string x2 = (endPoint.X - refPointX).ToString();
                    string y2 = (endPoint.Y - refPointY).ToString();

                    //utils.debug.alert(G10_Path);
                    //utils.debug.alert(incomingGuid.ToString());
                    //utils.debug.alert(incomingLayerName);

                    try
                    {
                        System.IO.File.AppendAllText(G10_Path, "0|" + incomingGuid.ToString() + "|" + incomingLayerName + "|1|" + x1 + "," + y1 + "," + x2 + "," + y2 + Environment.NewLine);
                    }
                    catch (Exception e)
                    {
                        utils.debug.alert(e.ToString());
                    }

                    //utils.debug.alert("Pausing!!");
                }
                else if (newCurve.SpanCount > 1)
                {
                    //Treat as linear polyline.
                    //utils.debug.ping(0, "Linear polyline added!");

                    List<string> coords = new List<string>();

                    int spanCount = newCurve.SpanCount;
                    for (int i = 0; i < spanCount; i++)
                    {
                        double activeParameter = newCurve.SpanDomain(i).Min;
                        Rhino.Geometry.Point3d activePoint = newCurve.PointAt(activeParameter);

                        double activeX = activePoint.X;
                        coords.Add(activeX.ToString());

                        double activeY = activePoint.Y;
                        coords.Add(activeY.ToString());
                    }

                    string coordInfo = string.Join(",", coords);

                    System.IO.File.AppendAllText(G10_Path, "1|" + incomingGuid + "|" + incomingLayer + "|" + newCurve.SpanCount.ToString() + "|" + coordInfo + Environment.NewLine);
                }
            }
            else if (newCurve.Degree > 1)
            {
                if (newCurve.SpanCount == 1)
                {
                    if (newCurve.TryGetArc(out Rhino.Geometry.Arc newArc) == true)
                    {
                        //Treat as workable arc.
                        //utils.debug.ping(0, "Arc added!");

                        System.IO.File.AppendAllText(G10_Path, "2|" + incomingGuid + "|" + incomingLayer + "|" + Environment.NewLine);
                    }
                    else
                    {
                        //Treat as more difficult winding curve.
                        //utils.debug.ping(0, "Arbitrary curve added!");

                        System.IO.File.AppendAllText(G10_Path, "5|" + incomingGuid + "|" + incomingLayer + "|" + Environment.NewLine);
                    }
                }
                else if (newCurve.SpanCount > 1)
                {
                    //Treat as complicated-as-hell polycurve.
                    //utils.debug.ping(0, "Rude!");

                    System.IO.File.AppendAllText(G10_Path, "6|" + incomingGuid + "|" + incomingLayer + "|" + Environment.NewLine);
                }
            }
            else if (newCurve.TryGetCircle(out Rhino.Geometry.Circle newCircle) == true)
            {
                //newCircle.
                //utils.debug.ping(0, "Circle added!");

                System.IO.File.AppendAllText(G10_Path, "3|" + incomingGuid + "|" + incomingLayer + "|" + Environment.NewLine);
            }
            else if (newCurve.TryGetEllipse(out Rhino.Geometry.Ellipse newEllipse) == true)
            {
                //newEllipse.
                //utils.debug.ping(0, "Ellipse added!");

                System.IO.File.AppendAllText(G10_Path, "4|" + incomingGuid + "|" + incomingLayer + "|" + Environment.NewLine);
            }
        }

        public static void removals(int state, Rhino.DocObjects.RhinoObject geo)
        {
            //Verify geometery is being added, not transformed.
            string G00_Path = utils.file_structure.getPathFor("G00");
            int verifyState = Convert.ToInt32(System.IO.File.ReadAllText(G00_Path));

            if (verifyState == 3)
            {
                state = 3;
            }
            else
            {
                System.IO.File.WriteAllText(G00_Path, state.ToString());
            }

            //Determine and sanitize cache file for geometry removals.
            string G20_Path = utils.file_structure.getPathFor("G20");

            if (System.IO.File.Exists(G20_Path) == false)
            {
                System.IO.File.Create(G20_Path);
            }

            //Cache deleted items to G20.
            System.IO.File.AppendAllText(G20_Path, geo.Id.ToString() + Environment.NewLine);
        }
    }
}
