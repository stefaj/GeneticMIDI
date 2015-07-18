using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Frequency
{
    public class ChromaticToneDuration : MetricFrequency
    {

        public override void GenerateFrequencies(Note[] notes)
        {
            foreach (Note n in notes)
            {
                if(n.NotePitch>=0 &&n.Velocity>0)
                    Add(new Pair(n.NotePitch, n.Duration));
            }
        }
    }
}
