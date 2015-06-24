using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Types
{
    public class MelodicBigram : MetricFrequency
    {
        public override void GenerateFrequencies(Note[] notes)
        {
            List<int> intervals = new List<int>();
            for (int i = 0; i < notes.Length - 1; i++)
            {
                int interval = Math.Abs(notes[i].Pitch - notes[i + 1].Pitch);
                intervals.Add(interval);
            }
            for(int i = 0; i < intervals.Count-2; i+=2)
            {
                int i1 = intervals[i];
                int i2 = intervals[i+1];
                Pair bigram = new Pair(i1, i2);
                base.Add(bigram);
            }
        }
    }
}
