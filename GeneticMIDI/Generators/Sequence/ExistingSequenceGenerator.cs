using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    public class ExistingSequenceGenerator : INoteGenerator
    {
        MelodySequence sequence;
        PatchNames instrument;
        public ExistingSequenceGenerator(MelodySequence sequence, PatchNames instrument)
        {
            this.sequence = sequence;
            this.instrument = instrument;
        }

        public MelodySequence Generate()
        {
            return sequence;
        }

        public MelodySequence Next()
        {
            return Generate();
        }

        public bool HasNext
        {
            get { return true; }
        }

        public PatchNames Instrument
        {
            get { return instrument; }
        }
    }
}
