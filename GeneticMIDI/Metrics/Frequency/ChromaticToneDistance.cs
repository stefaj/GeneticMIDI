using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Frequency
{
    /// <summary>
    /// Time intervals between chromatic tone repetitions
    /// </summary>
    public class ChromaticToneDistance : MetricFrequency
    {
        public override void GenerateFrequencies(Note[] notes)
        {
            for(int i = 0; i < notes.Count(); i++)
            {
                int distance = 0;
                for(int j = i+1; j < notes.Count(); j++)
                {
                    distance += notes[j].Duration;
                    if(notes[i].NotePitch == notes[j].NotePitch)
                    {
                        Add(new Pair(distance));
                        break;
                    }
                }
            }
        }
    }
}
