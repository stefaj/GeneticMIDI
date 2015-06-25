using Accord.Statistics.Models.Markov.Learning;
using AForge.Genetic;
using GeneticMIDI.FitnessFunctions;
using GeneticMIDI.Generators;
using GeneticMIDI.Metrics.Types;
using GeneticMIDI.Representation;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticMIDI
{
    class Program
    {
        static void Main(string[] args)
        {


            MusicPlayer player = new MusicPlayer();

            MelodySequence seq = new MelodySequence();
            for (int i = 0; i < 100; i++)
            {
                double s = Math.Sin(4 * Math.PI / 100 * (double)i);
                int pitch = 12 * 4 + (int)(s * 24);
                seq.AddNote(new Note(pitch, (int)Durations.en));
            }

            Composition comp = new Composition();
            comp.LoadFromMIDI(@"test\other\frere.mid");

            player.Play(comp);
            Console.ReadLine();


           
            player.Play(seq);


            return;











            //Chord Test


            //player.PlayChords(new Chord[] { dminor, dmajor, cmajor });

       










            if(args.Length == 0)
            {
                Test1();
               // Test4();
                return;
            }

            switch (args[0])
            {
                case "1":
                    Test1();
                    break;
                case "2":
                    Test2();
                    break;
                case "3":
                    Test3();
                    break;
                case "4":
                    Test4();
                    break;
                case "5":
                    Test5();
                    break;
                default:
                    Test1();
                    Test3();
                    break;
            }
         

            Console.ReadLine();
  
        }

        static void Test1()
        {
            Console.WriteLine("Hidden Markov Model");
            MarkovGenerator markov = new MarkovGenerator(@"test");
            var notes3 = markov.Generate();
            Play(notes3);
        }

        static void Test2()
        {
            Console.WriteLine("Normalized Compression Distance");
            NCD ncd = new NCD(@"test");
            GeneticGenerator evolver = new GeneticGenerator(ncd);
            var notes = evolver.Generate();
            Play(notes);
        }

        static void Test3()
        {
            Console.WriteLine("Chromatic Tone and Duration");
            //ChromaticToneDuration, ChromaticTone, MelodicBigram, RhythmicBigram
            CosineSimiliarity cosine = new CosineSimiliarity(@"test\harry.mid", new ChromaticToneDuration());
            GeneticGenerator evolver = new GeneticGenerator(cosine);
            var notes = evolver.Generate();
            Play(notes);

        }


        static void Test4()
        {
            Console.WriteLine("Melodic Bigram");
            //ChromaticToneDuration, ChromaticTone, MelodicBigram, RhythmicBigram
            CosineSimiliarity cosine = new CosineSimiliarity(@"test\harry.mid", new MelodicBigram());
            GeneticGenerator evolver = new GeneticGenerator(cosine);
            var notes = evolver.Generate();
            Play(notes);

        }

        static void Test5()
        {
            Console.WriteLine("Cross Correlation");
            CrossCorrelation corr = new CrossCorrelation(@"test\harry.mid");
            GeneticGenerator evolver = new GeneticGenerator(corr);
            var notes = evolver.Generate();
            Play(notes);
        }

        static void ChordTest()
        {
            MusicPlayer player = new MusicPlayer();
            Chord dminor = new Chord(new NoteNames[] { NoteNames.D, NoteNames.F, NoteNames.A }, 4, Durations.wn);
            Chord dmajor = new Chord(new NoteNames[] { NoteNames.G, NoteNames.B, NoteNames.B }, 4, Durations.wn);
            Chord cmajor = new Chord(new NoteNames[] { NoteNames.C, NoteNames.E, NoteNames.G }, 4, Durations.bn);

            HarmonySequence seq = new HarmonySequence();
            seq.AddChord(dminor); seq.AddChord(dmajor); seq.AddChord(cmajor); seq.AddChord(dminor); seq.AddChord(cmajor); seq.AddChord(dminor); seq.AddChord(dmajor); seq.AddChord(cmajor);
            player.Play(seq);
            player.Close();
        }

        static void Play(IEnumerable<Note> notes)
        {
            MusicPlayer player = new MusicPlayer();
            player.SetPatch(0, 1);
            player.PlayNotes(notes);
            player.Close();
        }

       
        
    }
}
