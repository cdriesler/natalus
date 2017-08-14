using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Grasshopper;

using echo;

namespace natalus.illustrator
{
    public class nata_ai
    {
        //Determine what geometry needs to be changed in Illustrator. Update .nata file accordingly, push indexes to Illustrator.
        public static void curve(List<Rhino.Geometry.Curve> geo, List<int> types, int count, string deltaFile)
        {
            //Declare item count vars.
            int line_count = 0;
            int polyline_count = 0;
            int circle_count = 0;

            //Read delta file.
            string deltaFileData = System.IO.File.ReadAllText(deltaFile);
            int prevCount = Convert.ToInt32(deltaFileData);
            int delta = count - prevCount;
            System.IO.File.WriteAllText(deltaFile, count.ToString());

            //Debug: declare change.
            //string debugMessage = count.ToString() + " - " + prevCount.ToString() +  " | Change is: " + delta.ToString();
            //
            //echo.interop echo = new interop();
            //echo.locate(0, debugMessage);

            //If delta is positive, run additive systems.
            //If delta is zero, run transformative systems.
            //If delta is negative, run deletion systems.
        } 

        private static void line(List<Rhino.Geometry.Curve> curves)
        {
            //Line conversion.
        }

        private static void polyline(List<Rhino.Geometry.Curve> curves)
        {
            //Linear polyline conversion.
        }

        private static void circle(List<Rhino.Geometry.Circle> curves)
        {
            //Circle converstion.
        }
    }
}
