using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Features
{
    class KeyPrevalence : IMetric
    {
        public Dictionary<Pair, float> Generate(Note[] notes)
        {
            Dictionary<Pair, float> dic = new Dictionary<Pair, float>();
           for(int i = 0; i < 12; i++)
           {
               int[] chords = StandardKeys.MAJOR_KEYS[i];
               float sum = 0;
               foreach(Note n in notes)
                    if(chords.Contains(n.NotePitch))
                        sum+=1;
               dic[new Pair(i)] = sum / notes.Length;
                              
           }
           return dic;
        }
    }
}
