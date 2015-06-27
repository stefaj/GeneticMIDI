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
        public Dictionary<object, float> Generate(Note[] notes)
        {
            Dictionary<object, float> dic = new Dictionary<object, float>();
           for(int i = 0; i < 12; i++)
           {
               int[] chords = StandardChords.MAJOR_CHORDS[i];
               float sum = 0;
               foreach(Note n in notes)
                    if(chords.Contains(n.NotePitch))
                        sum+=1;
               dic[i] = sum / notes.Length;
                              
           }
           return dic;
        }
    }
}
