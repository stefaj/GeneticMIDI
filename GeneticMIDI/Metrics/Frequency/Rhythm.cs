using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Frequency
{
    public class Rhythm : MetricFrequency
    {

        public override void GenerateFrequencies(Note[] notes)
        {
            foreach (Note n in notes)
            {
                int duration = n.Duration;
                Add(new Pair(duration));
            }
        }
    }
}
