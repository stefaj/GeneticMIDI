using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Frequency
{
    /// <summary>
    /// Pairs of adjacent rhythmic intervals
    /// </summary>
    public class RhythmicBigram : MetricFrequency
    {
        public override void GenerateFrequencies(Note[] notes)
        {
            var intervals = new List<float>();


            for (int i = 0; i < notes.Length - 1; i++)
            {
                float duration1 = notes[i].Duration;
                int j = i + 1;
                for (; j < notes.Length; j++)
                {
                    if (!notes[j].IsRest())
                        break;
                    else
                        duration1 += notes[j].Duration;
                }

                if (j >= notes.Length - 1)
                    break;

                float duration2 = notes[j].Duration;
                j++;
                for (; j < notes.Length; j++)
                {
                    if (!notes[j].IsRest())
                        break;
                    else
                        duration2 += notes[j].Duration;
                }
                float rel = duration1 / duration2;
                intervals.Add(rel);

                if (j >= notes.Length - 1)
                    break;
            }
            

            for (int i = 0; i < intervals.Count-2; i += 2)
            {
                float i1 = intervals[i];
                float i2 = intervals[i + 1];
                Pair bigram = new Pair(i1, i2);
                base.Add(bigram);
            }
        }
    }
}
