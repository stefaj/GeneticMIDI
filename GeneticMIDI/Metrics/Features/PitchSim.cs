using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Features
{
    class PitchSim : IMetric
    {
        public Dictionary<Pair, float> Generate(Representation.Note[] notes)
        {
            Dictionary<Pair, float> dic = new Dictionary<Pair, float>();
            float time = 0;
            foreach (Note n in notes)
            {

                dic[new Pair(time)] = n.Pitch;
                time += n.Pitch;

            }
            return dic;
        }
    }
}
