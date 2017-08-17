using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natalus
{
    public enum states
    {
        n0,     //Exit state machine. Idle.
        n1,     //Determine status of files. Is this a first time use? Newly opened? Actively running?
        n2,     //Prepare .nata files based on determination in n1.
        n3,     //Determine type of change in file. (deltaState)
        n4_0,   //
        n4_1,
        n4_2,
        n5
    }

    public class nata_state_machine
    {
        public static void nata()
        {
            natalus.states state = states.n1;

            while (state != states.n0)
            {
                switch(state)
                {
                    case states.n1:
                        //Some logic.
                        state = states.n2;
                        break;

                    case states.n2:
                        //Some logic.
                        state = states.n3;
                        break;
                }
            }
        }
    }
}
