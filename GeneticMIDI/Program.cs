using Accord.Statistics.Models.Markov.Learning;
using AForge.Genetic;
using GeneticMIDI.FitnessFunctions;
using GeneticMIDI.Generators;
using GeneticMIDI.Generators.CompositionGenerator;
using GeneticMIDI.Generators.Harmony;
using GeneticMIDI.Generators.NoteGenerators;
using GeneticMIDI.Metrics;
using GeneticMIDI.Metrics.Features;
using GeneticMIDI.Metrics.Frequency;
using GeneticMIDI.Output;
using GeneticMIDI.Representation;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticMIDI
{
    class Program
    {
        static void Main(string[] args)
        {

            
           /* Composition com = Composition.LoadFromMIDI(@"C:\Users\1gn1t0r\Documents\Visual Studio 2013\Projects\GeneticMIDI\Visualizer\bin\Debug\test\bach\bwv651.mid");
            var am = ActivityMatrix.GenerateFromComposition(com);
            Console.WriteLine("gg");



                return;*/
            foreach (var s in Utils.GetFiles(@"D:\Sync\4th year\Midi\Library\Anime", 3))
                Console.WriteLine(s);

            Console.ReadLine();

            return;
            
            InstrumentalGenerator gen = new InstrumentalGenerator(@"D:\Sync\4th year\Midi\scraped");
            

            Console.WriteLine("Generating");
            System.Random rand = new Random();
            var seed = rand.Next();

            //var comp = gen.Generate(seed);
            var comp = gen.Generate(new PatchNames[] { PatchNames.Flute, PatchNames.String_Ensemble_1, PatchNames.Synth_Voice });

            Console.WriteLine("Playing seed {0}", seed);

            MusicPlayer player = new MusicPlayer();


            foreach (var t in comp.Tracks)
                Console.WriteLine(t.Instrument.ToString());

            player.Play(comp);
  
  
        }

        static void LoadSongTest()
        {
            Console.WriteLine("Playing test ff7tifa");
            MusicPlayer player = new MusicPlayer();
            Composition c = Composition.LoadFromMIDI("test/other/ff7tifa.mid");
            player.Play(c);
        }

        static void DemoTest1()
        {
            Console.WriteLine("Hidden Markov Model, constant duration, twinkle");
            var mel1 = GetMelodySequence(@"test\harry.mid");
            var stoch = new StochasticGenerator(new MelodySequence[] { mel1 });
            var notes3 = stoch.Generate();
            Play(notes3);
            return;

        }

        static void DemoTest2()
        {
            MusicPlayer player = new MusicPlayer();
            Composition comp = new Composition();
            Track track1 = new Track(PatchNames.Acoustic_Grand, 1);
            Track track2 = new Track(PatchNames.Cello, 2);
            Console.WriteLine("Hidden Markov Model, constant duration, twinkle");
            var mel1 = GetMelodySequence(@"test\harry.mid");
            var stoch = new StochasticGenerator(new MelodySequence[] { mel1 });
            track1.AddSequence(stoch.Generate());
            track2.AddSequence(stoch.Generate());
            //comp.Add(track1);
            comp.Add(track2);
            player.Play(comp);
            return;

        }

        static void Test1()
        {
            /*Console.WriteLine("Hidden Markov Model");
            MarkovGenerator markov = new MarkovGenerator(@"test");
            var notes3 = markov.Generate();
            Play(notes3);*/
        }

        static void Test2()
        {
            /*Console.WriteLine("Normalized Compression Distance");
            NCD ncd = new NCD(@"test");
            GeneticGenerator evolver = new GeneticGenerator(ncd);
            var notes = evolver.Generate();
            Play(notes);*/
        }

        static void CosineChromaticTest()
        {
            Console.WriteLine("Chromatic Tone and Duration");
            //ChromaticToneDuration, ChromaticTone, MelodicBigram, RhythmicBigram
            var mel = GetMelodySequence(@"test\harry.mid");
            MetricSimilarity cosine = new MetricSimilarity(mel, new IMetric[]{new ChromaticToneDuration(), new DurationSim()});
            GeneticGenerator evolver = new GeneticGenerator(cosine, mel);
            var notes = evolver.Generate();
            Play(notes, PatchNames.Music_Box);
        }
        static void CosineTest()
        {
            Console.WriteLine("Chromatic Tone and Duration");
            //ChromaticToneDuration, ChromaticTone, MelodicBigram, RhythmicBigram
            MetricSimilarity cosine = new MetricSimilarity(GetMelodySequence(@"test\frere.mid"),
                new IMetric[] { new ChromaticToneDuration(), new MelodicBigram(), new RhythmicBigram() });
            GeneticGenerator evolver = new GeneticGenerator(cosine);
            var notes = evolver.Generate();
            Console.ReadLine();
            Play(notes);
        }


        static void Test4()
        {
            Console.WriteLine("Melodic Bigram");
            //ChromaticToneDuration, ChromaticTone, MelodicBigram, RhythmicBigram
            MetricSimilarity cosine = new MetricSimilarity(GetMelodySequence(@"test\harry.mid"), new MelodicBigram());
            GeneticGenerator evolver = new GeneticGenerator(cosine);
            var notes = evolver.Generate();
            Play(notes);

        }

        static void Test5()
        {
            Console.WriteLine("Cross Correlation");
            CrossCorrelation corr = new CrossCorrelation(GetMelodySequence(@"test\harry.mid"));
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

        static void Test8()
        {
            MusicPlayer player = new MusicPlayer();

            Console.WriteLine("Hidden Markov Model");
            var m1 = GetMelodySequence(@"test\other\frere.mid");
            var m2 = GetMelodySequence(@"test\other\babaa.mid");
            var m3 = GetMelodySequence(@"test\other\twinkle.mid");

            var m4 = GetMelodySequence(@"test\hagrid.mid");
            var m5 = GetMelodySequence(@"test\harry.mid");
            var m6 = GetMelodySequence(@"test\harry2.mid");
            MarkovGenerator markov = new MarkovGenerator(new MelodySequence[] { m6 });
            while (true)
            {
                Composition comp = new Composition();

                GeneticGenerator gen = new GeneticGenerator(new MetricSimilarity(m6, new IMetric[] { new Rhythm(), new RhythmicBigram(), new RhythmicInterval() }));
                var notes2 = gen.Generate();
                Track t2 = new Track((PatchNames)0, 10);
                MelodySequence s = new MelodySequence();
                s.AddPause((int)Durations.wn * 10);
                foreach (Note n in notes2.Notes)
                {
                    if (n.Pitch <= 0)
                        n.Pitch = 0;
                    else if (n.Pitch > 48)
                        n.Pitch = 40;
                    else if (n.Pitch > 70)
                        n.Pitch = 35;
                    else
                    {
                        n.Pitch = 49;
                        n.Duration *= 4;
                        n.Velocity = 50;
                        s.AddPause(n.Duration * 2);
                    }
                    s.AddNote(n);
                }
                t2.AddSequence(s);

                var notes3 = markov.Generate().Notes as Note[];

                MelodySequence baseseq = new MelodySequence();

                int max_dur = 0;
                int max_index = 0;
                for (int i = (int)(notes3.Length * 0.7f); i < notes3.Length; i++)
                {
                    if (notes3[i].Duration > max_dur)
                    {
                        max_dur = notes3[i].Duration;
                        max_index = i;
                    }
                }
                Note last_note = null;
                for (int i = 0; i < max_index; i++)
                {
                    last_note = notes3[i];
                    baseseq.AddNote(last_note);
                }
                baseseq.AddNote(new Note(last_note.Pitch - 12, last_note.Duration));
                baseseq.AddNote(new Note(last_note.Pitch - 24, last_note.Duration * 2));
                baseseq.AddPause(last_note.Duration * 32);



                Track t = new Track(PatchNames.Vibraphone, 1);

                var b1 = baseseq.Clone() as MelodySequence;
                var b2 = baseseq.Clone() as MelodySequence;
                var b3 = baseseq.Clone() as MelodySequence;
                b1.Transpose(12);
                b2.Transpose(6);
                b3.Transpose(-12);
                t.AddSequence(baseseq);
                t.AddSequence(b1);
                t.AddSequence(b2);
                t.AddSequence(b3);

                comp.Add(t);
                comp.Add(t2);
                Console.WriteLine("Press enter key to listen");
                Console.ReadLine();
                player.Play(comp);
            }
        }

        static void Test9()
        {

            MusicPlayer player = new MusicPlayer();
            Console.WriteLine("Harmony Test");
            //ChromaticToneDuration, ChromaticTone, MelodicBigram, RhythmicBigram
            MetricSimilarity cosine = new MetricSimilarity(GetMelodySequence(@"test\other\ff7tifa.mid"), new ChromaticToneDuration());
            GeneticGenerator evolver = new GeneticGenerator(cosine);
            var notes = evolver.Generate();
            SimpleChord ch = new SimpleChord();
            var melody = notes;
            HarmonySequence harmony = ch.GetHarmonySequence(melody);
            Track t1 = new Track(PatchNames.Acoustic_Grand, 2);
            t1.AddSequence(melody);
            Track t2 = new Track(PatchNames.Church_Organ, 1);
            t2.AddSequence(harmony);
            Composition comp = new Composition();
            // comp.Add(t1); 
            comp.Add(t2);
            player.Play(comp);
            Console.ReadLine();
            return;

        }


        static void ChordTest2()
        {
            MusicPlayer player = new MusicPlayer();
            //player.SetPatch((int)PatchNames.Overdriven_Guitar, 1);
            var a = new Chord(new NoteNames[] { NoteNames.C, NoteNames.E, NoteNames.G }, 4, Durations.hn);
            var b = new Chord(new NoteNames[] { NoteNames.Cs, NoteNames.E, NoteNames.GS }, 4, Durations.hn);
            var c = new Chord(new NoteNames[] { NoteNames.D, NoteNames.F, NoteNames.A }, 4, Durations.hn);
            var d = new Chord(new NoteNames[] { NoteNames.E, NoteNames.G, NoteNames.B }, 4, Durations.hn);
            var e = new Chord(new NoteNames[] { NoteNames.F, NoteNames.A, NoteNames.C }, 4, Durations.hn);
            var f = new Chord(new NoteNames[] { NoteNames.G, NoteNames.B, NoteNames.D }, 4, Durations.hn);
            var g = new Chord(new NoteNames[] { NoteNames.A, NoteNames.C, NoteNames.E }, 4, Durations.hn);
            var h = new Chord(new NoteNames[] { NoteNames.B, NoteNames.D, NoteNames.F }, 4, Durations.hn);
            var k = new Chord[] { a, b, c, d, e, f, g, h };
            Random r = new Random();
            while (true)
            {
                var chord = k[r.Next(0, k.Length)];
                chord.Duration = (int)((Durations)Math.Pow(2, r.Next(2, 6)));
                player.PlayChord(chord);

            }
        }

        static void Play(IEnumerable<Note> notes, PatchNames instrument = PatchNames.Vibraphone)
        {
            MusicPlayer player = new MusicPlayer();
            player.SetPatch((int)instrument, 1);
            player.PlayNotes(notes);
            player.Close();
        }

        static void Play(MelodySequence notes, PatchNames instrument = PatchNames.Vibraphone)
        {
            MusicPlayer player = new MusicPlayer();
            player.SetPatch((int)instrument, 1);
            player.Play(notes);
            player.Close();
        }


        static MelodySequence GetMelodySequence(string file)
        {
            Composition comp = Composition.LoadFromMIDI(file);
            return comp.GetLongestTrack().GetMainSequence() as MelodySequence;
        }

        static void FMajorTest()
        {
            MusicPlayer player2 = new MusicPlayer();
            Random r = new Random();
            for (int i = 0; i < 20; i++ )
            {
                int c = StandardKeys.C_MAJOR[r.Next(0, StandardKeys.F_MAJOR.Length)];
                player2.PlayNotes(new Note[] { new Note((NoteNames)c, 4 + r.Next(0, 2), Durations.qn) });
            }
            int d = StandardKeys.C_MAJOR[r.Next(0, StandardKeys.F_MAJOR.Length)];
            player2.PlayNotes(new Note[] { new Note((NoteNames)d, 5, Durations.hn) });
            d = StandardKeys.C_MAJOR[r.Next(0, StandardKeys.F_MAJOR.Length)];
            player2.PlayNotes(new Note[] { new Note((NoteNames)d, 3, Durations.wn) });

        }

       
        
    }
}
