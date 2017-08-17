using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//NOTE: This is a sketch template.

namespace natalus
{
    public enum gStates
    {
        G0A,     //Exit state machine. Idle.
        G1A,     //Determine status of files. Is this a first time use? Newly opened? Actively running?
        G2A,     //Prepare .nata files based on determination in n1.
        G3A,     //Determine type of change in file. (deltaState)
        G4A,     //
        G4B,
        G4C,
        G5A
    }

    public enum sStates
    {
        S0A,
        S1A,
        S2A
    }

    public class nata_state_machine
    {
        public static void nata()
        {
            //Declare NATA file structure in Grasshopper UserObjects folder.



            gStates gState = gStates.G0A;

            while (gState != gStates.G0A)
            {
                switch(gState)
                {
                    case gStates.G1A:
                        //Some logic.
                        gState = gStates.G2A;
                        break;

                    case gStates.G2A:
                        //Some logic.
                        
                        break;
                }
            }
        }
    }
}
