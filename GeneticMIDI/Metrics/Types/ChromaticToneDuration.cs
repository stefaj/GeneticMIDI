using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Types
{
    class ChromaticToneDuration : MetricFrequency
    {

        public override void GenerateFrequencies(Note[] notes)
        {
            foreach (Note n in notes)
            {
                Add(new Pair(n.NotePitch, n.Duration));
            }
        }
    }
}
