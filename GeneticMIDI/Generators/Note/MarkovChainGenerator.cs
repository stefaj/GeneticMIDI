using GeneticMIDI.Representation;
using Markov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.NoteGenerators
{
    public class MarkovChainGenerator : IGenerator
    {
        MarkovChain<Note> chain;

        public int MaxLength { get; private set; }

        public event OnFitnessUpdate OnPercentage;
        public MarkovChainGenerator(int order = 3, int length = 150)
        {
            chain = new MarkovChain<Note>(order);
            MaxLength = length;
        }

        public void AddMelody(string path)
        {
            Composition comp = new Composition();
            comp.LoadFromMIDI(path);

            var m = comp.GetLongestTrack().GetMainSequence() as MelodySequence;
            m.TrimLeadingRests();
            Representation.Note[] notes = m.ToArray();
            chain.Add(notes);
        }

        public void AddMelody(MelodySequence seq)
        {
            Representation.Note[] notes = seq.ToArray();

            chain.Add(notes);
        }

        public IEnumerable<Note> Generate()
        {
            var gen_ = chain.Chain();
            var gen = new List<Note>();

            int i = 0;
            foreach (Note n in gen_)
            {
                gen.Add(n);
                if (i++ > MaxLength)
                    break;
            }

            return gen;
        }
    }
}
