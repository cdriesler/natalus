using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Grasshopper.Kernel;

namespace guano
{
    public class new_item_test : GH_Component
    {
        public new_item_test() : base("New Item", "NewItem", "Returns GUID of all new items.", "Natalus", "debug")
        {
        }

protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
{
    //pManager.Add *
        }

protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
{
            pManager.AddTextParameter("GUIDs", "g", "Latest GUIDS", GH_ParamAccess.list);
        }

protected override void SolveInstance(IGH_DataAccess DA)
{
            RhinoDoc.AddRhinoObject += OnObjectAdded;

            List<string> id = new List<string>();
        }

        public static void OnObjectAdded(object sender, RhinoObjectEventArgs ea)
        {
        for (int i = 0, i < ea.TheObject.       
        }

        public override Guid ComponentGuid
{
    //get { return new Guid("GUID-HERE"); }
        }
    }


}


