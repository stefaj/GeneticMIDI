using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Types
{
    class Pitch : MetricFrequency
    {

        public override void GenerateFrequencies(Note[] notes)
        {
            foreach(Note n in notes)
            {
                int pitch = n.Pitch;
                Add(new Pair(pitch));
            }
        }
    }


}
