using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GeneticMIDI.Metrics.Frequency
{
    /// <summary>
    /// Relationship between adjacent note rhythms
    /// </summary>
    public class RhythmicInterval : MetricFrequency
    {
        public override void GenerateFrequencies(Note[] notes)
        {
            for (int i = 0; i < notes.Length - 1; i++)
            {
                float duration1 = notes[i].Duration;
                int j = i+1;
                for (; j < notes.Length; j++)
                {
                    if (!notes[j].IsRest())
                        break;
                    else
                        duration1 += notes[j].Duration;
                }

                float duration2 = notes[j].Duration;
                j++;
                for(; j < notes.Length; j++)
                {
                    if (!notes[j].IsRest())
                        break;
                    else
                        duration2 += notes[j].Duration;
                }
                float rel = duration1 / duration2;
                Add(new Pair(rel));

                if (j >= notes.Length - 1)
                    break;
            }
        }
    }
}
