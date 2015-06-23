using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Types
{
    public class RhythmicBigram : MetricFrequency
    {
        public override void GenerateFrequencies(Note[] notes)
        {
            List<int> intervals = new List<int>();
            for (int i = 0; i < notes.Length - 1; i++)
            {
                int interval = Math.Abs(notes[i].Duration - notes[i + 1].Duration);
                intervals.Add(interval);
            }
            for (int i = 0; i < intervals.Count-2; i += 2)
            {
                int i1 = intervals[i];
                int i2 = intervals[i + 1];
                Pair bigram = new Pair(i1, i2);
                base.Add(bigram);
            }
        }
    }
}
