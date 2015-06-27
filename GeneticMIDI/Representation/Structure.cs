using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class Structure
    {
   
        public int Bars{get; private set;}

        public int BeatsPerBar { get; private set; }

        public int TicksPerBeat { get; private set; }

        public int TicksPerBar { get; private set; }

        public int Ticks { get; private set; }

        public int MaxVelocity { get; private set; }


        public Structure(int bars, int beatsPerBar, int ticksPerBeat, int maxVelocity)
        {
            this.Bars = bars;
            this.BeatsPerBar = beatsPerBar;
            this.TicksPerBeat = ticksPerBeat;
            this.MaxVelocity = maxVelocity;

            this.TicksPerBar = beatsPerBar * ticksPerBeat;
            this.Ticks = bars * TicksPerBar;
        }

    }
}
