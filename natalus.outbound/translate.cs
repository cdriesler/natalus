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

            int layerIndex = geo.Attributes.LayerIndex;
            Rhino.DocObjects.Layer incomingLayer = Rhino.RhinoDoc.ActiveDoc.Layers[layerIndex];
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

            //New type key:
            //0 - linear curve
            //1 - linear polyline
            //2 - any non-linear curve
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

                        double activeX = activePoint.X - refPointX;
                        coords.Add(activeX.ToString());

                        double activeY = activePoint.Y - refPointY;
                        coords.Add(activeY.ToString());

                        if (i == spanCount - 1)
                        {
                            //On reaching final span, also record endpoint.
                            double finalParameter = newCurve.SpanDomain(i).Max;
                            Rhino.Geometry.Point3d finalPoint = newCurve.PointAt(finalParameter);

                            double finalX = finalPoint.X - refPointX;
                            coords.Add(finalX.ToString());

                            double finalY = finalPoint.Y - refPointY;
                            coords.Add(finalY.ToString());
                        }
                    }

                    string coordInfo = string.Join(",", coords);

                    System.IO.File.AppendAllText(G10_Path, "1|" + incomingGuid.ToString() + "|" + incomingLayerName + "|" + newCurve.SpanCount.ToString() + "|" + coordInfo + Environment.NewLine);
                }
            }
            else if (newCurve.Degree > 1)
            {
                //Generate degree 3 approximation of incoming curve. (Low tolerance makes it more than "good enough")
                Rhino.Geometry.BezierCurve[] bCurveSpans = Rhino.Geometry.BezierCurve.CreateCubicBeziers(newCurve, 0.01, 0.01);
                int bCurveCount = bCurveSpans.Length;

                List<string> bPointData = new List<string>();
                List<double> bPointCache = new List<double>();

                //Cache point info.
                for (int i = 0; i < bCurveCount; i++)
                {
                    //Starting point caching process.
                    if (i == 0)
                    {
                        //Parse anchor point data.
                        Rhino.Geometry.Point3d activeAnchor = bCurveSpans[i].GetControlVertex3d(0);
                        double activeAnchorX = activeAnchor.X - refPointX;
                        double activeAnchorY = activeAnchor.Y - refPointY;
                        bPointCache.Add(activeAnchorX);
                        bPointCache.Add(activeAnchorY);

                        //Parse left direction data. For starting point, this is the same as the anchor.
                        bPointCache.Add(activeAnchorX);
                        bPointCache.Add(activeAnchorY);

                        //Parse right direction data.
                        Rhino.Geometry.Point3d activeRightDirection = bCurveSpans[i].GetControlVertex3d(1);
                        double activeRightDirectionX = activeRightDirection.X - refPointX;
                        double activeRightDirectionY = activeRightDirection.Y - refPointY;
                        bPointCache.Add(activeRightDirectionX);
                        bPointCache.Add(activeRightDirectionY);

                        //Push packaged point data to final list.
                        string bPoint = string.Join(":", bPointCache);
                        bPointData.Add(bPoint);
                        bPointCache.Clear();
                    }
                    //Intermediate point caching process.
                    else
                    {
                        //Parse anchor point data.
                        Rhino.Geometry.Point3d activeAnchor = bCurveSpans[i].GetControlVertex3d(0);
                        double activeAnchorX = activeAnchor.X - refPointX;
                        double activeAnchorY = activeAnchor.Y - refPointY;
                        bPointCache.Add(activeAnchorX);
                        bPointCache.Add(activeAnchorY);

                        //Parse left direction data.
                        Rhino.Geometry.Point3d activeLeftDirection = bCurveSpans[i - 1].GetControlVertex3d(2);
                        double activeLeftDirectionX = activeLeftDirection.X - refPointX;
                        double activeLeftDirectionY = activeLeftDirection.Y - refPointY;
                        bPointCache.Add(activeLeftDirectionX);
                        bPointCache.Add(activeLeftDirectionY);

                        //Parse right direction data.
                        Rhino.Geometry.Point3d activeRightDirection = bCurveSpans[i].GetControlVertex3d(1);
                        double activeRightDirectionX = activeRightDirection.X - refPointX;
                        double activeRightDirectionY = activeRightDirection.Y - refPointY;
                        bPointCache.Add(activeRightDirectionX);
                        bPointCache.Add(activeRightDirectionY);

                        //Push packaged point data to final list.
                        string bPoint = string.Join(":", bPointCache);
                        bPointData.Add(bPoint);
                        bPointCache.Clear();
                    }
                }

                //Cache endpoint info after loop finishes.
                //Parse anchor point data.
                Rhino.Geometry.Point3d lastActiveAnchor = bCurveSpans[bCurveCount - 1].GetControlVertex3d(3);
                double lastActiveAnchorX = lastActiveAnchor.X - refPointX;
                double lastActiveAnchorY = lastActiveAnchor.Y - refPointY;
                bPointCache.Add(lastActiveAnchorX);
                bPointCache.Add(lastActiveAnchorY);

                //Parse left direction data.
                Rhino.Geometry.Point3d lastActiveLeftDirection = bCurveSpans[bCurveCount - 1].GetControlVertex3d(2);
                double lastActiveLeftDirectionX = lastActiveLeftDirection.X - refPointX;
                double lastActiveLeftDirectionY = lastActiveLeftDirection.Y - refPointY;
                bPointCache.Add(lastActiveLeftDirectionX);
                bPointCache.Add(lastActiveLeftDirectionY);

                //Parse right direction data. For the ending point, this is the same as the anchor.
                bPointCache.Add(lastActiveAnchorX);
                bPointCache.Add(lastActiveAnchorY);

                //Push packaged point data to final list.
                string lastBPoint = string.Join(":", bPointCache);
                bPointData.Add(lastBPoint);
                bPointCache.Clear();

                //Finalize and record data.
                string coordInfo = string.Join(";", bPointData);

                System.IO.File.AppendAllText(G10_Path, "2|" + incomingGuid.ToString() + "|" + incomingLayerName + "|" + newCurve.SpanCount.ToString() + "|" + coordInfo + Environment.NewLine);
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

            //Cache deleted items to G20.
            System.IO.File.AppendAllText(G20_Path, geo.Id.ToString() + Environment.NewLine);
        }

        public static void attributes(Rhino.DocObjects.RhinoModifyObjectAttributesEventArgs ea)
        {
            //Translate attributes.

            //Tranformation codes:
            //1     layer
            //2     color
            //4     stroke weight
            //8     x
            //16    x
            //
            //Ex: attDeltaCode = 3 means layer and color were updated

            //Track what attributes just changed.
            int attDeltaCode = 0;

            if (ea.NewAttributes.LayerIndex != ea.OldAttributes.LayerIndex)
            {
                attDeltaCode = attDeltaCode + 1;
            }
            
            if (ea.NewAttributes.ObjectColor != ea.OldAttributes.ObjectColor)
            {
                attDeltaCode = attDeltaCode + 2;
            }

            if (ea.NewAttributes.PlotWeight != ea.OldAttributes.PlotWeight)
            {
                attDeltaCode = attDeltaCode + 4;
            }

            //utils.debug.alert(Convert.ToString(attDeltaCode, 2));
        }
    }
}
