using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics
{
    public interface IMetric
    {
        //Vector
        Dictionary<object, float> Generate(Note[] notes);
    }
}
