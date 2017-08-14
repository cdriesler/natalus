using System;
using Rhino;
using Grasshopper.Kernel;

namespace project_name
{
    public class component_name : GH_Component
    {
        public component_name() : base("NAME", "LABEL", "DESC", "TAB", "SUB-TAB")
        {
        }

protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
{
    //pManager.Add *
        }

protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
{
    //pManager.Add *
        }

protected override void SolveInstance(IGH_DataAccess DA)
{
    //magic
        }

public override Guid ComponentGuid
{
    //get { return new Guid("GUID-HERE"); }
        }
    }
}
