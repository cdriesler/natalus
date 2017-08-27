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
                string docBoxTest = utils.properties.tryGetDocBox();

                if (docBoxTest == "error")
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fatal error locating artboard reference in Rhino.");
                }
            }
            else if (sendBool == false)
            {
                utils.properties.setPushState(sendBool);

                //"Hide" docbox.
                string D10_Path = utils.file_structure.getPathFor("D10");
                if (System.IO.File.Exists(D10_Path) == false)
                {
                    //Do nothing, docbox has not yet been created.
                }
                else
                {
                    Guid docBoxID = new Guid(utils.properties.tryGetDocBox());
                    RhinoDoc.ActiveDoc.Objects.Delete(docBoxID, true);

                    string D30_Path = utils.file_structure.getPathFor("D30");
                    if (System.IO.File.Exists(D30_Path))
                    {
                        Guid labelID = new Guid(System.IO.File.ReadAllText(D30_Path));
                        RhinoDoc.ActiveDoc.Objects.Delete(labelID, true);
                    }
                }
            }

            //Event handlers constellation.

            //SELECTION
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

            //GEOMETRY
            RhinoDoc.AddRhinoObject -= (sender, e) => OnObjectAdded(e);
            RhinoDoc.AddRhinoObject += (sender, e) => OnObjectAdded(e);

            RhinoDoc.DeleteRhinoObject -= (sender, e) => OnObjectRemoved(e);
            RhinoDoc.DeleteRhinoObject += (sender, e) => OnObjectRemoved(e);

            RhinoDoc.UndeleteRhinoObject -= (sender, e) => OnObjectUndelete(e);
            RhinoDoc.UndeleteRhinoObject += (sender, e) => OnObjectUndelete(e);

            /* TRANSFORMATION
            RhinoDoc.BeforeTransformObjects -= (sender, ea) => OnBeforeTransform(ea);
            RhinoDoc.BeforeTransformObjects += (sender, ea) => OnBeforeTransform(ea);
            */

            RhinoDoc.ReplaceRhinoObject -= (sender, ea) => ReplaceObjUpdateState();
            RhinoDoc.ReplaceRhinoObject += (sender, ea) => ReplaceObjUpdateState();

            RhinoDoc.BeforeTransformObjects -= (sender, ea) => TransformUpdateState(ea);
            RhinoDoc.BeforeTransformObjects += (sender, ea) => TransformUpdateState(ea);


            //When doc saved, copy critical existing data if file name changed. (x10, D01, D10)
            //RhinoDoc.BeginSaveDocument
            //ea.FileName is new save name, can be used for new path to copy to.

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
                //Parse changes in geometry.
                string geoDeltaState = utils.file_structure.getPathFor("G00");
                int geoState = 0;
                int selState = 0;
                if (System.IO.File.Exists(geoDeltaState) == false)
                {
                    geoState = 0;
                }
                else if (System.IO.File.Exists(geoDeltaState) == true)
                {
                    geoState = Convert.ToInt32(System.IO.File.ReadAllText(geoDeltaState));
                }

                //If RhinoApp.Idle fires after no changes in geometry, G00 state will be 0.
                if (geoState > 0 && geoState < 5)
                {
                    outbound.push.geometryToIllustrator(geoState);
                }
                else if (geoState == 5)
                {

                }

                //Parse changes in selection.
                string selectionState = utils.file_structure.getPathFor("S00");
                if (System.IO.File.Exists(selectionState) == false)
                {
                    selState = 0;
                }
                //Freeze selection changes if goemetry is changing. (Nasty collisions occur with many commands.)
                else if (System.IO.File.Exists(selectionState) == true && geoState == 0)
                {
                    selState = Convert.ToInt32(System.IO.File.ReadAllText(selectionState));
                }

                //If RhinoApp.Idle fires after no changes in selection, S00 state will be 0.
                if (selState > 0)
                {
                    outbound.push.selectionToIllustrator(selState);
                }

            }
        }

        public void TransformUpdateState(RhinoTransformObjectsEventArgs ea)
        {
            bool sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                //debug.alert("Tranformation event!");
                
                //TODO: Reset document if artboard reference moves.

                string G00_Path = utils.file_structure.getPathFor("G00");
                System.IO.File.WriteAllText(G00_Path, "3");
            }
        }

        public void ReplaceObjUpdateState()
        {
            bool sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                //debug.alert("Object replaced!");

                string G00_Path = utils.file_structure.getPathFor("G00");
                System.IO.File.WriteAllText(G00_Path, "3");
            }
        }

        public void OnObjectAdded(RhinoObjectEventArgs ea)
        {
            bool sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                //debug.alert("Addition event!");

                //Check if change being processed is the docbox.
                string docBoxID = utils.properties.tryGetDocBox();
                if (ea.ObjectId.ToString() == docBoxID)
                {
                    //Cache reference point.
                    Rhino.Geometry.Point3d refPoint = utils.properties.getRefPoint();

                    string D20_Path = utils.file_structure.getPathFor("D20");
                    System.IO.File.WriteAllText(D20_Path, refPoint.X.ToString() + "," + refPoint.Y.ToString());

                    outbound.push.docBoxChanges(ea.TheObject);
                }
                else
                {
                    string G00_Path = utils.file_structure.getPathFor("G00");
                    if (System.IO.File.Exists(G00_Path) == false)
                    {
                        System.IO.File.WriteAllText(G00_Path, "1");
                    }

                    //Verify object to parse is a curve before translating.
                    if (ea.TheObject.Geometry.ObjectType.ToString().ToLower().Contains("curve"))
                    {
                        outbound.translate.curves(1, ea.TheObject);
                    }
                    else
                    {
                        //Added object is not a curve, do nothing.
                    }
                }
            }
        }

        public void OnObjectUndelete(RhinoObjectEventArgs ea)
        {
            //Treat like object added, without the docbox.
            bool sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            if (sendBool == true)
            {
                //debug.alert("Undelete event!");

                string G00_Path = utils.file_structure.getPathFor("G00");
                if (System.IO.File.Exists(G00_Path) == false)
                {
                    System.IO.File.WriteAllText(G00_Path, "3");
                }

                //Check if the objects being undeleted is the docBox. (May mean an undone transformation.)
                string D12_Path = utils.file_structure.getPathFor("D12");
                string prevDocBoxID = "";
                if (System.IO.File.Exists(D12_Path) == false)
                {
                    //Do nothing.
                }
                else if (System.IO.File.Exists(D12_Path) == true)
                {
                    prevDocBoxID = System.IO.File.ReadAllText(D12_Path);
                }

                //Do some correction if undone object is docBox.
                if (ea.TheObject.Id.ToString() == prevDocBoxID)
                {
                    //debug.alert("DocBox transformation has been undone!");
                    //Correct D10.NATA after it was changed by OnDelete.
                    string D10_Path = utils.file_structure.getPathFor("D10");
                    System.IO.File.WriteAllText(D10_Path, prevDocBoxID);

                    //Update illustrator artboard.
                    outbound.push.docBoxChanges(ea.TheObject);
                }
                //Otherwise, translate like normal.
                else
                {
                    //Verify object to parse is a curve before translating.
                    if (ea.TheObject.Geometry.ObjectType.ToString().ToLower().Contains("curve"))
                    {
                        outbound.translate.curves(3, ea.TheObject);
                    }
                    else
                    {
                        //Added object is not a curve, do nothing.
                    }
                }
            }
        }

        public void OnObjectRemoved(RhinoObjectEventArgs ea)
        {
            bool sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            if (sendBool == true)
            {
                //debug.alert("Removal event!");

                //
                string D10_Path = utils.file_structure.getPathFor("D10");
                if (System.IO.File.Exists(D10_Path) == false)
                {
                    //Do nothing.
                    //debug.alert("D10 does not exist.");
                }
                //If objects being removed is the docbox:
                else if (System.IO.File.ReadAllText(D10_Path) == ea.TheObject.Id.ToString())
                {
                    //utils.debug.alert("DocBox was deleted!");

                    //Remove temporary label.
                    Guid labelID = new Guid(utils.properties.tryGetDocBoxLabel());
                    RhinoDoc.ActiveDoc.Objects.Delete(labelID, true);

                    //Record its GUID under D11, previous docBox information.
                    string D11_Path = utils.file_structure.getPathFor("D11");

                    if (System.IO.File.Exists(D11_Path) == true)
                    {
                        //If D11 exists, cache it for UnDelete check.
                        string D12_Path = utils.file_structure.getPathFor("D12");
                        System.IO.File.WriteAllText(D12_Path, System.IO.File.ReadAllText(D11_Path));
                    }

                    System.IO.File.WriteAllText(D11_Path, ea.TheObject.Id.ToString());

                    //Recreate the box.
                    utils.properties.tryGetDocBox();
                }
                //If not, record removal event:
                else
                {
                    string G00_Path = utils.file_structure.getPathFor("G00");
                    if (System.IO.File.Exists(G00_Path) == false)
                    {
                        System.IO.File.WriteAllText(G00_Path, "2");
                    }

                    //Verify object to parse is a curve before translating.
                    if (ea.TheObject.Geometry.ObjectType.ToString().ToLower().Contains("curve"))
                    {
                        outbound.translate.removals(2, ea.TheObject);
                    }
                    else
                    {
                        //Added object is not a curve, do nothing.
                    }
                }
            }
        }
    }
}
