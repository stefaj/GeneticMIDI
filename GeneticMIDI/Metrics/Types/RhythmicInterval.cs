using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Types
{
    public class RhythmicInterval : MetricFrequency
    {
        public override void GenerateFrequencies(Note[] notes)
        {
            for (int i = 0; i < notes.Length - 1; i++)
            {
                int interval = Math.Abs(notes[i].Duration - notes[i + 1].Duration);
                Add(new Pair(interval));
            }
        }
    }
}
