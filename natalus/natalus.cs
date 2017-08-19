using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.Input.Custom;
using Rhino.Display;
using Grasshopper.Kernel;

//Project references.
using echo;
using natalus.inbound;
using natalus.outbound;
using natalus.utils;

namespace natalus
{
    public class natalus : GH_Component
    {
        public natalus() : base("Natalus", "NATA", "Maintains information handshake between Rhino and Illustrator.", "natalus", "main")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Send", "->", "Set to true to allow live sync. Use the button component to manually update.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Receive", "<-", "Toggle to pull unsynced data from illustrator.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //No outputs needed?
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ////STARTUP
            //Verify existence of javascript library.

            //TODO. Unsure of best way to ship jsx files. .zip file structure like so? User unzips, drops in Libraries folder.
            //
            //-Natalus/
            //--echo.dll
            //--natalus.gha
            //--natalus.utils.dll
            //--natalus.outbound.dll
            //--natalus.inbound.dll
            //--NATA/
            //--JSX/
            //---S10.jsx
            //---S20.jsx
            //---etc.
            
            ////OUTBOUND
            //Check if outbound data is allowed.
            bool sendBool = false;
            DA.GetData(0, ref sendBool);

            if (sendBool == true)
            {
                utils.properties.setPushState(sendBool);

                //Initialize docbox!
                string docBoxID = utils.properties.getDocBoxID();
            }
            else if (sendBool == false)
            {
                utils.properties.setPushState(sendBool);

                //Hide docbox.
            }

            //Event handlers constellation.
            RhinoDoc.DeselectObjects -= (sender, e1) => OnSelectionDecrease(sender, e1, sendBool);
            RhinoDoc.DeselectObjects += (sender, e1) => OnSelectionDecrease(sender, e1, sendBool);
            //RhinoDoc.DeleteRhinoObject += (sender, e2) => ImplicitSelectionDecrease(sender, e2, sendBool);

            RhinoDoc.SelectObjects -= (sender, e1) => OnSelectionIncrease(sender, e1, sendBool);
            RhinoDoc.SelectObjects += (sender, e1) => OnSelectionIncrease(sender, e1, sendBool);
            //RhinoDoc.UndeleteRhinoObject += (sender, e2) => ImplicitSelectionIncrease(sender, e2, sendBool);

            RhinoDoc.DeselectAllObjects -= (sender, e) => OnSelectionReset(sendBool);
            RhinoDoc.DeselectAllObjects += (sender, e) => OnSelectionReset(sendBool);

            RhinoApp.Idle -= (sender, e) => OnIdle(sendBool);
            RhinoApp.Idle += (sender, e) => OnIdle(sendBool);

            ////INBOUND
            //Begin inbound data process if requested.
            bool receiveBool = false;
            DA.GetData(1, ref receiveBool);

            //Pull data from illustrator.
            if (receiveBool == true)
            {
                //Pause all outbound data if receiving to prevent endless loop.
                //Really, really, really hope it works this way.
                utils.properties.setPushState(false);

                //Pull data from illustrator.
            }
            else if (receiveBool == false)
            {
                //If not receiving data, let sending boolean be what's set by the user.
                utils.properties.setPushState(sendBool);
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2e60063e-1a4b-4b61-96ea-d93f90dc7d2f"); }
        }

        ////Selection event functions.
        public void OnSelectionIncrease(object sender, RhinoObjectSelectionEventArgs ea, bool sendBool)
        {
            sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                outbound.push.selectionIncrease(ea.RhinoObjects);
            }
        }

        public void ImplicitSelectionIncrease(object sender, RhinoObjectEventArgs ea, bool sendBool)
        {
            sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                Rhino.DocObjects.RhinoObject[] newObj = new Rhino.DocObjects.RhinoObject[1];
                newObj[0] = ea.TheObject;

                outbound.push.selectionIncrease(newObj);
            }
        }

        public void OnSelectionDecrease(object sender, RhinoObjectSelectionEventArgs ea, bool sendBool)
        {
            sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                outbound.push.selectionDecrease(ea.RhinoObjects);
            }
        }

        public void ImplicitSelectionDecrease(object sender, RhinoObjectEventArgs ea, bool sendBool)
        {
            sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                Rhino.DocObjects.RhinoObject[] newObj = new Rhino.DocObjects.RhinoObject[1];
                newObj[0] = ea.TheObject;

                outbound.push.selectionDecrease(newObj);
            }
        }

        public void OnSelectionReset(bool sendBool)
        {
            sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                outbound.push.selectionReset();
            }
        }

        public void OnIdle(bool sendBool)
        {
            sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                string selectionState = utils.file_structure.getPathFor("S00");
                int state = Convert.ToInt32(System.IO.File.ReadAllText(selectionState));

                //If RhinoApp.Idle fires after no changes in selection, S00 state will be 0.
                if (state > 0)
                {
                    outbound.push.selectionToIllustrator(state);
                }
            }
        }
    }
}
