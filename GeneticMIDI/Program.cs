using Accord.MachineLearning.Bayes;
using Accord.Neuro.Learning;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using AForge;
using AForge.Genetic;
using AForge.Neuro;
using AForge.Neuro.Learning;
using DotNetLearn.Data;
using DotNetLearn.Markov;
using GeneticMIDI.FitnessFunctions;
using GeneticMIDI.Fractal;
using GeneticMIDI.Generators;
using GeneticMIDI.Generators.CompositionGenerator;
using GeneticMIDI.Generators.Harmony;
using GeneticMIDI.Generators.NoteGenerators;
using GeneticMIDI.Generators.Sequence;
using GeneticMIDI.Metrics;
using GeneticMIDI.Metrics.Features;
using GeneticMIDI.Metrics.Frequency;
using GeneticMIDI.Output;
using GeneticMIDI.Representation;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            /*Feed forward neural network
Group notes into 3
Classify all groups in existing songs as 1, all other combinations as 0.

Use as fitness function


Use HMM decode as fitness function GA*/

           /* Databank db = new Databank("lib");

            List<MelodySequence> drumSeqs = new List<MelodySequence>();
            var comps = new Composition[][] { db.Load("Classic Rock").Compositions, db.Load("Jazz").Compositions, Utils.LoadCompositionsParallel(@"D:\Sync\4th year\Midi\Library2\Drums") };

            foreach(Composition[] catt in comps)
            {
                foreach(var c in catt)
                {
                    foreach(var t in c.Tracks)
                    {
                        if(t.Channel == 10)
                        {
                            drumSeqs.Add(t.GetMainSequence() as MelodySequence);
                        }
                    }
                }
            }

            List<Composition> drums = new List<Composition>();
            foreach(var m in drumSeqs)
            {
                Composition c = new Composition();
                Track t = new Track(PatchNames.Acoustic_Grand, 10);
                t.AddSequence(m);
                c.Add(t);
                drums.Add(c);
            }

            var catdrum = new CompositionCategory("Drums", "lib/Drums", drums.ToArray());
            catdrum.Save("lib/Drums");
            Console.ReadLine();
            */





            /*Composition comp = Composition.LoadFromMIDI(@"C:\Users\1gn1t0r\Documents\git\GeneticMIDI\GeneticMIDI\bin\Debug\test\other\twinkle.mid");
            float time = Note.ToRealDuration(comp.GetLongestTrack().Duration);
            Console.WriteLine("Total time: {0}", time);
            MusicPlayer player = new MusicPlayer();
            player.Play(comp);
            Console.ReadLine();*/




            Databank db = new Databank("lib");
            var cat = db.Load("Jazz");

            Codebook<Note> book = new Codebook<Note>();

            List<int[]> sequences = new List<int[]>();

            foreach(var comp in cat.Compositions)
            {
                foreach(var track in comp.Tracks)
                {
                    if(track.Instrument == PatchNames.Trumpet)
                    {
                        var mel = track.GetMelodySequence();
                        mel.Trim(80);
                        mel.StandardizeDuration();
                        
                        book.Add(mel.Notes);
                        sequences.Add(book.ToCodes(mel.Notes));
                    }
                }
                if (sequences.Count > 10)
                    break;
            }

            DotNetLearn.Markov.HiddenMarkovModel hmm = new DotNetLearn.Markov.HiddenMarkovModel(100, book.TotalUniqueSymbols);
            hmm.UniformRandomPriors();
            hmm.MaxIterations = 500;
            hmm.Train(sequences.ToArray());


            var o = hmm.PredictObservationSequence(80);
            var newmel = new MelodySequence(book.Translate(o));

            Track t = new Track(PatchNames.Trumpet, 2);
            t.AddSequence(newmel);

            Console.ReadLine();
            MusicPlayer player = new MusicPlayer();
            player.Play(t);

            player.Stop();

            Console.ReadLine();

            /*
            var gen = new SamplingWithReplacement(cat, PatchNames.Acoustic_Grand);

            
            var mel = gen.Generate();


            Console.WriteLine(mel.ToString());
            MusicPlayer player = new MusicPlayer();
            player.Play(mel);


            Console.WriteLine("Press enter to save");
            Console.ReadLine();
            WriteMelodyToFile(gen.OriginalSequence, "sampling_in.mid");
            WriteMelodyToFile(mel, "sampling_out.mid");
            */









                /*  MusicPlayer player = new MusicPlayer();
            
                  //db.LoadAll();

                  AccompanyGeneratorMarkov genMark = new AccompanyGeneratorMarkov(cat);
                  var targetSeq = cat.Compositions[2].GetLongestTrack().GetMainSequence() as MelodySequence;
                  var seqTest = genMark.Generate(targetSeq,5);

                  Track trackTest = new Track(PatchNames.Orchestral_Strings, 2); trackTest.AddSequence(seqTest);

                  Track targetTrack = new Track(PatchNames.Acoustic_Grand, 3); targetTrack.AddSequence(targetSeq);

                  Composition comp = new Composition(); 
                  comp.Add(trackTest);
                  comp.Add(targetTrack);

                  player.Play(comp);

            
                  return;*/




                /*   Databank db = new Databank("lib");
                   var cat = db.Load("Classical");
                   //AccompanimentGenerator2 Test
                   AccompanimentGenerator2 gen = new AccompanimentGenerator2(cat, PatchNames.Orchestral_Strings);
                   gen.Train();
                   //
                   Composition comp = Composition.LoadFromMIDI(@"D:\Sync\4th year\Midi\Library2\Classical\Mixed\dvorak.mid");
                   //var comp = Composition.LoadFromMIDI(@"C:\Users\1gn1t0r\Documents\git\GeneticMIDI\GeneticMIDI\bin\Debug\test\ff7tifa.mid");
                   gen.SetSequence(comp.Tracks[0].GetMainSequence() as MelodySequence);

            

                   MusicPlayer player = new MusicPlayer();

      
                       Console.WriteLine("Press enter to listen");
                       Console.ReadLine();

                       var mel = gen.Generate();
                       Composition newComp = new Composition();
                       Track newTrack = new Track(PatchNames.Orchestral_Strings , 2);
                       newTrack.AddSequence(mel);
                       newComp.Add(newTrack);
                       /*comp.Tracks[0].Instrument = PatchNames.Acoustic_Grand; (comp.Tracks[0].GetMainSequence() as MelodySequence).ScaleVelocity(0.8f);
                       /* newComp.Tracks.Add(comp.Tracks[0]);


                       player.Play(newComp);*/



                Console.ReadLine();
        }

        /// <summary>
        /// Generates a category and saves to a file
        /// </summary>
        /// <param name="categoryName"></param>
        static void GenerateCategories()
        {
            Databank.GenerateCategories(@"D:\Sync\4th year\Midi\Library2", "lib");
        }

        static void WriteMelodyToFile(MelodySequence seq, string filename)
        {
            Composition comp = new Composition();
            Track track = new Track(PatchNames.Acoustic_Grand, 1);
            track.AddSequence(seq);
            comp.Add(track);
            comp.WriteToMidi(filename);
        }

        static void MetricTimingTest()
        {

            Databank db = new Databank("lib");
            var cat = db.Load("Classical");        
            

            var fmets = new IMetric[]{new ChromaticTone(), new ChromaticToneDistance(), new ChromaticToneDuration(), new MelodicBigram(), new MelodicInterval()
            , new Pitch(), new Rhythm(), new RhythmicBigram(), new RhythmicInterval()};


            MusicPlayer player = new MusicPlayer();
            foreach (var m in fmets)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var metric = MetricSimilarity.GenerateMetricSimilarityMulti(cat.Compositions, new IMetric[] { m });

                GeneticGenerator gen = new GeneticGenerator(metric);
                gen.PrintProgress = false;
                gen.MaxGenerations = 1000;
                var mel = gen.Generate();
                mel.Trim(20);
                watch.Stop();
                var className = ((object)m).GetType().Name;
                Console.WriteLine("{0} - MF: {1}, AF {2}, T {3}", className, gen.MaxFitness, gen.AvgFitness, watch.ElapsedMilliseconds);
                Console.ReadLine();
                player.Play(mel);

            }

            Console.ReadLine();
        }

        static void GetPopularInstruments()
        {
            Databank db = new Databank("lib");
            var cat = db.Load("Classical");
            var cat2 = db.Load("Jazz");

            Dictionary<PatchNames, int> instrumentsCount = new Dictionary<PatchNames, int>();

            foreach (var c in cat.Compositions)
            {
                foreach (var t in c.Tracks)
                {
                    if (instrumentsCount.ContainsKey(t.Instrument))
                        instrumentsCount[t.Instrument]++;
                    else
                        instrumentsCount[t.Instrument] = 1;
                }
            }
            foreach (var c in cat2.Compositions)
            {
                foreach (var t in c.Tracks)
                {
                    if (instrumentsCount.ContainsKey(t.Instrument))
                        instrumentsCount[t.Instrument]++;
                    else
                        instrumentsCount[t.Instrument] = 1;
                }
            }

            int n = 10;
            var myList = instrumentsCount.ToList();

            myList.Sort((firstPair, nextPair) =>
            {
                return firstPair.Value.CompareTo(nextPair.Value);
            }
            );

            List<PatchNames> instrs = new List<PatchNames>();
            for (int i = 0; i < n; i++)
            {
                instrs.Add(myList[myList.Count - i - 1].Key);
            }
            instrs.ToArray();
            Console.Write("var pop = new PatchNames[]{");
            foreach (var i in instrs)
                Console.Write("PatchNames." + i.ToString() + ",");
            Console.Write("};");
            Console.ReadLine();
        }


        static void TrainANNFFAccomp()
        {
            Console.WriteLine("This application trains accompaniment neural networks");
            var cats = Databank.GetCategories();

            Databank db = new Databank("lib");

            var popularInstruments = new PatchNames[]{PatchNames.Acoustic_Grand,PatchNames.String_Ensemble_1,
                PatchNames.Acoustic_Bass,PatchNames.Trumpet,PatchNames.Vibraphone,PatchNames.Electric_Grand,
                PatchNames.French_Horn,PatchNames.Flute,PatchNames.Trombone,PatchNames.Music_Box};

            foreach (var catName in cats)
            {

                Console.WriteLine("Category {0}", catName);
                var cat = db.Load(catName);

                if (cat.Compositions.Length > 3000)
                    Console.WriteLine("Skipping category {0} due to length", cat.CategoryName);


                foreach (var instr in popularInstruments)
                {
                    Console.WriteLine("Training ANN for {0} - {1}, {2} epochs", cat.ToString(), instr.ToString(), 6000);

                    try
                    {
                        AccompanimentGeneratorANNFF ann = new AccompanimentGeneratorANNFF(cat, instr);
                        ann.Epochs = 5000;
                        ann.Train();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }

            }

            Console.ReadLine();
        }
        static void BrownTest()
        {
            var gen = new ReflectingBrownNoteGenerator(new NoteRangeRestrictor(48, 72, 2, 16, Scales.ScaleTypes[1]), new Random(), -1, 1, -1, 1);


            MelodySequence seq = gen.Generate();
            Console.WriteLine(seq.ToString());
            MusicPlayer player = new MusicPlayer();
            player.Play(seq);

            Console.ReadLine();
        }
        static void ANNFFNAccomp()
        {
            Databank db = new Databank("lib");
            var cat = db.Load("Classical");

            Composition inputComp = cat.Compositions[6];
            MelodySequence inputSeq = inputComp.Tracks[0].GetMainSequence() as MelodySequence;

            AccompanimentGeneratorANNFF gen = new AccompanimentGeneratorANNFF(cat, PatchNames.Acoustic_Grand);
            //gen.Initialize();
            gen.SetSequence(inputSeq);
            gen.Train();

            var outMel = gen.Generate();

            MusicPlayer player = new MusicPlayer();


            Composition comp = new Composition();
            Track t = new Track(PatchNames.Orchestral_Strings, 6);
            t.AddSequence(outMel);
            comp.Add(t);
            comp.Add(inputComp.Tracks[0]);
            player.Play(comp);
            comp.WriteToMidi("ann_ff_accomp.mid");

        }

        static void LSTMAccompTest()
        {

       /*     Databank db = new Databank("lib");
            var cat = db.Load("Classical");

            Composition inputComp = cat.Compositions[3];
            MelodySequence inputSeq = inputComp.Tracks[0].GetMainSequence() as MelodySequence;

            AccompanimentGenerator gen = new AccompanimentGenerator(cat, PatchNames.Acoustic_Grand);
            gen.Initialize();
            gen.SetSequence(inputSeq);

            var outMel = gen.Generate();

            MusicPlayer player = new MusicPlayer();


            Composition comp = new Composition();
            Track t = new Track(PatchNames.Orchestral_Strings, 6);
            t.AddSequence(outMel);
            comp.Add(t);
            // comp.Add(inputComp.Tracks[0]);
            player.Play(comp);
            comp.WriteToMidi("lstm_accomp.mid");*/
        }

        static void MarkovAccompTest()
        {
            Databank db = new Databank("lib");
            var cat = db.Load("Classical");


            Composition inputComp = cat.Compositions[6];
            MelodySequence inputSeq = inputComp.Tracks[0].GetMainSequence() as MelodySequence;

            AccompanyGeneratorMarkov gen = new AccompanyGeneratorMarkov(cat, PatchNames.Orchestral_Strings);

            var outMel = gen.Generate(inputSeq, 10);

            MusicPlayer player = new MusicPlayer();


            Composition comp = new Composition();
            Track t = new Track(gen.Instrument, 6);
            t.AddSequence(outMel);
            comp.Add(t);
            comp.Add(inputComp.Tracks[0]);
            player.Play(comp);
            comp.WriteToMidi("markov_model_accomp.mid");

        }

        static void GeneticTest()
        {
            Databank db = new Databank("lib");
            var cat = db.Load("Classical");


            MetricSimilarity cosine = MetricSimilarity.GenerateMetricSimilarityMulti(cat.Compositions, new IMetric[] { new ChromaticToneDistance(), new MelodicInterval(), new ChromaticToneDuration(), new RhythmicBigram() });
            GeneticGenerator gen = new GeneticGenerator(cosine, PatchNames.Orchestral_Strings, cat);
            gen.OnPercentage += (sender, percentage, fitness) => { Console.WriteLine("{0}: {1}", percentage, fitness); };
            gen.MaxGenerations = 1000;
            var outMel = gen.Generate();

            MusicPlayer player = new MusicPlayer();
            player.Play(outMel);

            Composition comp = new Composition();
            Track t = new Track(gen.Instrument, 3);
            t.AddSequence(outMel);
            comp.Add(t);
            comp.WriteToMidi("genetic_cosine_all.mid");
        }

        static void InstrumentalTest()
        {
            Databank db = new Databank("lib");
            var cat = db.Load("Classical");

            InstrumentalGenerator gen = new InstrumentalGenerator(cat);
            gen.Initialize();
            var outMel = gen.GenerateInstrument(PatchNames.Acoustic_Grand, 40);

            MusicPlayer player = new MusicPlayer();
            player.Play(outMel);

            Composition comp = new Composition();
            Track t = new Track(PatchNames.Acoustic_Grand, 3);
            t.AddSequence(outMel);
            comp.Add(t);
            comp.WriteToMidi("instrumental_acousticgrand.mid");
        }

        static void NeuralNetworkAccompanimentTest()
        {

            // initialize input and output values
            double[][] input = new double[4][] {
            new double[] {0, 0}, new double[] {0, 1},
            new double[] {1, 0}, new double[] {1, 1}
            };
            double[][] output = new double[4][] {
            new double[] {0}, new double[] {1},
            new double[] {1}, new double[] {0}
            };
            SigmoidFunction sig = new SigmoidFunction();

            Accord.Neuro.Networks.RestrictedBoltzmannMachine boltz = new Accord.Neuro.Networks.RestrictedBoltzmannMachine(200, 200);

            // create neural network
            ActivationNetwork network = new ActivationNetwork(
                new SigmoidFunction(2),
                200,
                20,
                200);
            //BackPropagationLearning teacher = new BackPropagationLearning(network);
            //LevenbergMarquardtLearning teacher = new LevenbergMarquardtLearning(network);
            Accord.Neuro.Learning.ParallelResilientBackpropagationLearning teacher = new ParallelResilientBackpropagationLearning(network);
            Accord.Neuro.Networks.DeepBeliefNetwork dpn = new Accord.Neuro.Networks.DeepBeliefNetwork(200, 20);

            // teacher.IncreaseFactor = 1.01;
            Composition c = Composition.LoadFromMIDI("test/other/ff7tifa.mid");

            MusicPlayer player = new MusicPlayer();
            //player.Play(c.Tracks[0]);

            List<double[]> inputs = new List<double[]>(); List<double[]> outputs = new List<double[]>();

            inputs.Add(GetDoublesFromNotes((c.Tracks[0].GetMainSequence() as MelodySequence).ToArray()));
            outputs.Add(GetDoublesFromNotes((c.Tracks[1].GetMainSequence() as MelodySequence).ToArray()));

            //  inputs.Add(GetDoublesFromNotes((c.Tracks[1].GetMainSequence() as MelodySequence).ToArray()));
            //    outputs.Add(GetDoublesFromNotes((c.Tracks[2].GetMainSequence() as MelodySequence).ToArray()));

            // inputs.Add(GetDoublesFromNotes((c.Tracks[0].GetMainSequence() as MelodySequence).ToArray()));
            // outputs.Add(GetDoublesFromNotes((c.Tracks[3].GetMainSequence() as MelodySequence).ToArray()));

            int its = 0;
            while (its++ < 10000)
            {

                double error = teacher.RunEpoch(inputs.ToArray(), outputs.ToArray());
                Console.WriteLine("{0}: Error - {1}", its, error);
            }

            var input_melody = (c.Tracks[0].GetMainSequence() as MelodySequence);
            var new_notes = network.Compute(GetDoublesFromNotes(input_melody.ToArray()));

            var new_mel = GetMelodyFromDoubles(new_notes);

            player.Play(new_mel);

            Console.ReadLine();
        }

        static double[] GetDoublesFromNotes(Note[] notes, int max_length=200)
        {
            int length = notes.Length > max_length ? max_length : notes.Length;
            double[] outarr = new double[max_length];
            int max = 128 * 128 + 64;

            int i = 0;
            for(i = 0; i < length; i++)
            {
                int o = notes[i].Pitch * 128 + notes[i].Duration;
                outarr[i] = (double)o / (double)max;
            }
            for (; i < max_length; i++)
                outarr[i] = 0;
            return outarr;
        }

        static MelodySequence GetMelodyFromDoubles(double[] arr)
        {
            int max = 128 * 128 + 64;
            Note[] notes = new Note[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                int num = (int)(arr[i] * max);
                notes[i] = new Note(num / 128, num % 128);
            }
            return new MelodySequence(notes);
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
