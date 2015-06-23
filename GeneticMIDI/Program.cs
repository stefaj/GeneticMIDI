using Accord.Statistics.Models.Markov.Learning;
using AForge.Genetic;
using GeneticMIDI.FitnessFunctions;
using GeneticMIDI.Generators;
using GeneticMIDI.Metrics.Types;
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
            if(args.Length == 0)
            {
                Test1();
                Test3();
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
            CosineSimiliarity cosine = new CosineSimiliarity(@"test\frere.mid", new ChromaticToneDuration());
            GeneticGenerator evolver = new GeneticGenerator(cosine);
            var notes = evolver.Generate();
            Play(notes);

        }


        static void Test4()
        {
            Console.WriteLine("Melodic Bigram");
            //ChromaticToneDuration, ChromaticTone, MelodicBigram, RhythmicBigram
            CosineSimiliarity cosine = new CosineSimiliarity(@"test\frere.mid", new MelodicBigram());
            GeneticGenerator evolver = new GeneticGenerator(cosine);
            var notes = evolver.Generate();
            Play(notes);

        }

        static void Test5()
        {
            Console.WriteLine("Cross Correlation");
            CrossCorrelation corr = new CrossCorrelation(@"test\frere.mid");
            GeneticGenerator evolver = new GeneticGenerator(corr);
            var notes = evolver.Generate();
            Play(notes);

        }

        static void Play(IEnumerable<Note> notes, int octave=4)
        {
            MidiOut midiOut = new MidiOut(0);
            midiOut.Send(MidiMessage.ChangePatch(2, 1).RawData);
            foreach (Note n in notes)
            {
            
                midiOut.Send(MidiMessage.StartNote(n.Pitch, n.Volume, 1).RawData);
                Thread.Sleep(n.Duration*4*15);
                midiOut.Send(MidiMessage.StopNote(n.Pitch, 0, 1).RawData);

            }
            midiOut.Close();
            midiOut.Dispose();
        }
        
    }
}
