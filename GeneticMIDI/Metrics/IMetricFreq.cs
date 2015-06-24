using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics
{
    interface IMetric
    {
        Dictionary<Pair, int> Generate(Note[] notes);
    }
}
