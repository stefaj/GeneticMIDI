using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators
{
    public interface INoteGenerator
    {
        MelodySequence Generate();

        MelodySequence Next();

        bool HasNext { get; }

        PatchNames Instrument { get; }
    }
}
