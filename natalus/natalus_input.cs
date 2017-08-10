using System;
using Rhino;
using Grasshopper.Kernel;
using echo;

namespace natalus
{
    public class natalus_input : GH_Component
    {
        public natalus_input() : base("Natalus Input", "nInput", "Receives geometry information from rhino pipeline.", "natalus", "main")
        {
        }

protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
{
            //pManager.AddCurveParameter("geo_in", "G", "Incoming curve geometry. Please use the Geometry Pipeline component.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Activate", "a", "Test connection with illustrator.", GH_ParamAccess.item, false);
        }

protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
{
            pManager.AddIntegerParameter("confirmation", "c", "Did it work?", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool active = false;

            DA.SetData(0, 0);

            DA.GetData(0, ref active);

            if (active == true)
            {
                DA.SetData(0, 2);

                echo.interop echo = new interop();

                echo.locate(0, "hot potato");
            }
        }

public override Guid ComponentGuid
{
    get { return new Guid("d1440ded-c272-40a8-8486-54232cff159f"); }
}
    }
}
