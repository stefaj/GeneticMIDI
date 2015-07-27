using GeneticMIDI.Generators.NoteGenerators;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.CompositionGenerator
{
    public class CompositionGenerator : IPlaybackGenerator, ICompositionGenerator
    {
        Composition composition;
        //IGenerator trackGenerator;
        public CompositionGenerator(Composition comp)
        {
            this.composition = comp;
            //this.trackGenerator = trackGenerator;
        }

        public Composition Generate()
        {
            int trackNo = composition.Tracks.Count;
            Composition newComp = new Composition();

            Parallel.For(0, trackNo, i =>
                {
                    PatchNames instrument = composition.Tracks[i].Instrument;
                    Console.WriteLine("Generating track {0} with instrument {1}", i, instrument.ToString());
                    var melodySeq = composition.Tracks[i].GetMainSequence() as MelodySequence;

                    MarkovChainGenerator chain = new MarkovChainGenerator(2, melodySeq.Length);
                    chain.AddMelody(melodySeq);
                    var notes = chain.Generate();


                    Track track = new Track(instrument, (byte)(i + 1));
                    track.AddSequence(new MelodySequence(notes));
                    newComp.Add(track);
                    //var notes = trackGenerator.Generate();
                    Console.WriteLine("Done Generating track {0}", i);

                });

            return newComp;


        }

        public PlaybackInfo GeneratePlayback()
        {
            return Generate().GeneratePlaybackInfo();
        }

        public Composition GenerateComposition()
        {
            return Generate();
        }
    }
}
