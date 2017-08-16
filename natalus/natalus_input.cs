using System;
using System.Collections.Generic;
using Rhino;
using Grasshopper.Kernel;

//Project references.
using echo;
using natalus.illustrator;

namespace natalus
{
    public class natalus_input : GH_Component
    {
        public natalus_input() : base("Natalus Input", "nInput", "Receives geometry information from rhino pipeline.", "natalus", "main")
        {
        }

protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
{
            pManager.AddCurveParameter("Linework", "L", "All 2D geometry to draw. Please use the Geometry Pipeline component.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Activate", "A", "Set to true to allow live sync. Use the button component to manually update.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Debug", "d", "Toggle to test connection with Illustrator.", GH_ParamAccess.item, false);
        }

protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
{
            pManager.AddGeometryParameter("Tracked", "T+", "Tracked geometry.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Untracked", "T-", "Untracked geometry.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number of Curves", "n", "Absolute amount of curves currently synchronized.", GH_ParamAccess.item);
        }

protected override void SolveInstance(IGH_DataAccess DA)
{
            ////Startup utilities. 
            //Assign input booleans to state variables.
            bool activeBool = false;
            DA.GetData(1, ref activeBool);

            bool debugBool = false;
            DA.GetData(2, ref debugBool);

            //Continuation tokens.
            int C_1 = 0;

            //Establish filepaths of sync files. (C:\Users\<user>\AppData\Roaming\Grasshopper\UserObjects\Natalus\<3dm-name.nata>)
            if (RhinoDoc.ActiveDoc.Name.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please save the current file.");
                return;
            }

            string ghPath = Grasshopper.Folders.DefaultAssemblyFolder.Replace("Libraries\\", "UserObjects\\Natalus\\NATA\\");
            string docName = RhinoDoc.ActiveDoc.Name;

            string activeSyncDirectory = ghPath;
            string activeSyncFileName = docName.Replace(".3dm", "_curves.nata");
            string activeSyncDelta = docName.Replace(".3dm", "_delta.nata");
            string activeSyncStaging = docName.Replace(".3dm", "_staging.nata");
            string activeSyncRuntime = docName.Replace(".3dm", "_runtime.nata");

            string syncPath = activeSyncDirectory + activeSyncFileName;
            string deltaPath = activeSyncDirectory + activeSyncDelta;

            //Locate sync file on activation.
            if (activeBool == true)
            {
                //Attempt to set viewport display mode to installed natalus mode.
                try
                {
                    Rhino.Display.DisplayModeDescription nDisplay = Rhino.Display.DisplayModeDescription.FindByName("Natalus");
                    Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.DisplayMode = nDisplay;
                }
                catch
                {
                    //Reaction, if the display mode becomes important.
                }


                //Verify existence, or create new delta and main file if none exists.
                if (!System.IO.Directory.Exists(activeSyncDirectory))
                {
                    System.IO.Directory.CreateDirectory(activeSyncDirectory);
                    System.IO.File.AppendAllText(activeSyncDirectory + activeSyncFileName, "metadata placeholder" + Environment.NewLine);
                    System.IO.File.WriteAllText(activeSyncDirectory + activeSyncDelta, "0");
                    System.IO.File.WriteAllText(activeSyncDirectory + activeSyncRuntime, "0");
                }
                else
                {
                    if (!System.IO.File.Exists(activeSyncDirectory + activeSyncFileName))
                    {
                        System.IO.File.AppendAllText(activeSyncDirectory + activeSyncFileName, "metadata placeholder" + Environment.NewLine);
                        System.IO.File.WriteAllText(activeSyncDirectory + activeSyncDelta, "0");
                        System.IO.File.WriteAllText(activeSyncDirectory + activeSyncRuntime, "0");
                    }
                    //Reset delta file if latest object runtime is less than last recorded runtime. (File has been closed and reopened.)
                    else if (RhinoDoc.ActiveDoc.Objects.MostRecentObject().RuntimeSerialNumber <= Convert.ToInt32(System.IO.File.ReadAllText(activeSyncDirectory + activeSyncRuntime)))
                    {
                        System.IO.File.WriteAllText(activeSyncDirectory + activeSyncDelta, "0");
                    }
                    else
                    {
                        //System.IO.File.AppendAllText(activeSyncDirectory + activeSyncFileName, "session placeholder" + Environment.NewLine);
                    }
                }

                //Verify existence, or create data staging file.
                if (!System.IO.File.Exists(activeSyncDirectory + activeSyncStaging))
                {
                    System.IO.File.Create(activeSyncDirectory + activeSyncStaging);
                }
                else if (System.IO.File.Exists(activeSyncDirectory + activeSyncStaging))
                {
                    System.IO.File.WriteAllText(activeSyncDirectory + activeSyncStaging, "");
                }

                //Set continutation token to allow work with geometry.
                C_1 = 1;

            }
            else
            {
                //Set continutation token to not allow work with geometry.
                C_1 = 0;
            }

            ////Begin parsing of curve geometry.
            //Assign input geometry to curve list.
            List<Rhino.Geometry.Curve> allCurves = new List<Rhino.Geometry.Curve>();

            //Reset delta if geo input is empty.
            int delta = 0;
            if (!DA.GetDataList(0, allCurves))
            {
                delta = 0;
            }

            //Create lists for tracked and untracked items.
            List<Rhino.Geometry.Curve> trackedGeo = new List<Rhino.Geometry.Curve>();
            List<Rhino.Geometry.Curve> untrackedGeo = new List<Rhino.Geometry.Curve>();

            //Create curve type list. Index aligned to trackedGeo list.
            //0 - Linear curve
            //1 - Linear polyline
            //2 - Circle
            List<int> typeList = new List<int>();


            //Sort curve geometry into tracked and untracked lists.
            if (C_1 == 1)
            {
                for (int index = 0; index < allCurves.Count; index++)
                {
                    //Non-linear curve filter routine.
                    if (allCurves[index].Degree != 1)
                    {
                        if (allCurves[index].IsCircle())
                        {
                            trackedGeo.Add(allCurves[index]);
                            typeList.Add(2);
                        }
                        else
                        {
                            untrackedGeo.Add(allCurves[index]);
                        }
                    }
                    //Linear curve filter routine.
                    else if (allCurves[index].Degree == 1)
                    {
                        trackedGeo.Add(allCurves[index]);
                        if (allCurves[index].IsPolyline())
                        {
                            typeList.Add(1);
                        }
                        else
                        {
                            typeList.Add(0);
                        }
                    }
                }
                //Set output values on component.
                DA.SetDataList(0, trackedGeo);
                DA.SetDataList(1, untrackedGeo);
                DA.SetData(2, trackedGeo.Count);

                ////Define type of change document just experienced.
                //Read delta file for previous curve count. ***Delta file is NOT the latest change. (Rename delta to count?)
                string deltaFileData = System.IO.File.ReadAllText(deltaPath);
                int prevCount = Convert.ToInt32(deltaFileData);
                delta = trackedGeo.Count - prevCount;
                System.IO.File.WriteAllText(deltaPath, trackedGeo.Count.ToString());

                //Determine delta state.
                int deltaState = 0;

                //Geometry being added.
                if (delta > 0)
                {
                    deltaState = 0;
                }
                //Geometry being removed.
                else if (delta < 0)
                {
                    deltaState = 1;
                }
                //Geometry being changed. Note: Not attributes, position.
                else if (delta == 0)
                {
                    deltaState = 2;
                }

                //Grab past runtime before starting conversion.
                uint prev_runtime = Convert.ToUInt32(System.IO.File.ReadAllText(activeSyncDirectory + activeSyncRuntime));

                //Send curve profile to illustrator sync utility.
                string errorCode = nata_ai.curve(trackedGeo, typeList, delta, deltaState, prev_runtime);

                //Error handling from nata_ai process.
                if (errorCode.Contains("1"))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorCode);
                    return;
                }
                else if (errorCode.Contains("2"))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorCode);
                    return;
                }
                else if (errorCode.Contains("3"))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorCode);
                    return;
                }
            }

            //Generic error handling.
            else
            {
                if (activeBool == false)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set active to true to synchronize.");
                    return;
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot create sync file in UserObjects directory.");
                    return;
                }
                
            }

            ////Debug utility. 
            //Ping illustrator and tests information transfer.
            if (debugBool == true)
            {
                DA.SetData(2, 2);

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
