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

            RhinoDoc.BeforeTransformObjects -= (sender, ea) => TransformUpdateState();
            RhinoDoc.BeforeTransformObjects += (sender, ea) => TransformUpdateState();

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
                string selectionState = utils.file_structure.getPathFor("S00");
                int selState = 0;
                if (System.IO.File.Exists(selectionState) == false)
                {
                    selState = 0;
                }
                else if (System.IO.File.Exists(selectionState) == true)
                {
                    selState = Convert.ToInt32(System.IO.File.ReadAllText(selectionState));
                }

                //If RhinoApp.Idle fires after no changes in selection, S00 state will be 0.
                if (selState > 0)
                {
                    outbound.push.selectionToIllustrator(selState);
                }

                string geoDeltaState = utils.file_structure.getPathFor("G00");
                int geoState = 0;
                if (System.IO.File.Exists(geoDeltaState) == false)
                {
                    geoState = 0;
                }
                else if (System.IO.File.Exists(geoDeltaState) == true)
                {
                    geoState = Convert.ToInt32(System.IO.File.ReadAllText(geoDeltaState));
                }

                //If RhinoApp.Idle fires after no changes in geometry, G00 state will be 0.
                if (geoState > 0)
                {
                    outbound.push.geometryToIllustrator(geoState);
                }
            }
        }

        /* TODO: Redundant. Can streamline process by only checking for docbox id when it fires AddRhinoObject.
        public void OnBeforeTransform(RhinoTransformObjectsEventArgs ea)
        {
            bool sendBool = utils.properties.getPushState();

            //string debugMessage = ea.Transform.
            //debug.ping(0, debugMessage);

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
                //If only one objects changes, and it's the docbox, update illustrator bounds.
                if (ea.ObjectCount == 1)
                {
                    string docBoxID = utils.properties.getDocBoxID();
                    if (ea.Objects[0].Id.ToString() == docBoxID)
                    {
                        outbound.push.docBoxChanges(ea.Objects[0], ea);
                    }
                }
                else
                {
                    //Transform items normally.
                }
            }
        } */

        public void TransformUpdateState()
        {
            bool sendBool = utils.properties.getPushState();

            if (sendBool == false)
            {
                //Do nothing.
            }
            else if (sendBool == true)
            {
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
                /* TODO: Undoing a docbox transformation ditches it and creates a new one without deleting the old one.
                string D10_Path = utils.file_structure.getPathFor("D10");
                string D11_Path = utils.file_structure.getPathFor("D11");
                string docBoxCheck = "";
                    
                if (System.IO.File.Exists(D11_Path))
                {
                    docBoxCheck = System.IO.File.ReadAllText(D11_Path);
                }

                if (ea.TheObject.Id.ToString() == docBoxCheck)
                {
                    //If a docbox tranformation is undone, remove new replacement box and reset D10.
                    Guid objToDelete = new Guid(System.IO.File.ReadAllText(D10_Path));
                    Rhino.RhinoDoc.ActiveDoc.Objects.Delete(objToDelete, true);

                    System.IO.File.WriteAllText(D10_Path, docBoxCheck);
                }
                */

                //Check if change being processed is the docbox.
                string docBoxID = utils.properties.tryGetDocBox();
                if (ea.ObjectId.ToString() == docBoxID)
                {
                    outbound.push.docBoxChanges(ea.TheObject);
                }
                else
                {
                    string G00_Path = utils.file_structure.getPathFor("G00");
                    if (System.IO.File.Exists(G00_Path) == false)
                    {
                        System.IO.File.WriteAllText(G00_Path, "1");
                    }
                    outbound.translate.curves(1, ea.TheObject);
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
                string G00_Path = utils.file_structure.getPathFor("G00");
                if (System.IO.File.Exists(G00_Path) == false)
                {
                    System.IO.File.WriteAllText(G00_Path, "1");
                }
                outbound.translate.curves(1, ea.TheObject);
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
                //Check that docbox was not deleted.
                string docBoxID = utils.properties.tryGetDocBox();
                if (ea.ObjectId.ToString() == docBoxID)
                {
                    string geoStatePath = utils.file_structure.getPathFor("G00");
                    int geoDeltaState = Convert.ToInt32(System.IO.File.ReadAllText(geoStatePath));

                    //Transforming doxBox is okay and expected!
                    if (geoDeltaState != 3)
                    {
                        utils.debug.alert("The Illustrator artboard reference in Rhino has been deleted. Please undo and put it back.");
                    }
                }
                else
                {
                    string G00_Path = utils.file_structure.getPathFor("G00");
                    if (System.IO.File.Exists(G00_Path) == false)
                    {
                        System.IO.File.WriteAllText(G00_Path, "2");
                    }

                    outbound.translate.removals(2, ea.TheObject);
                }
            }
        }
    }
}
