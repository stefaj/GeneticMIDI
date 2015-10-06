    #if LSTM
using de.jannlab;
using de.jannlab.core;
using de.jannlab.data;
using de.jannlab.generator;
using de.jannlab.misc;
using de.jannlab.tools;
using de.jannlab.training;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{

    [Serializable]
    public class AccompanimentGenerator : INoteGenerator
    {
        java.util.Random rnd = new java.util.Random(0);

        // Constant stuff
        const int MAX_OUTPUTS = 2;
        const int MAX_INPUTS = 2;
        const int MAX_HIDDEN = 5;
        const int epochs = 100;
        const double learningrate = 0.05;
        const double momentum = 0.4;
        const double filterThreshold = 0.1;
        string SAVE_FILE
        {
            get
            {
                return "accomp" + ((int)instrument).ToString() + ".dat";
            }
        }
       
        MelodySequence sequence;
        Dictionary<Note, int> noteHashes = new Dictionary<Note, int>();
        int hash = 0;
        Net ann;
        PatchNames instrument;

        CompositionCategory category;

        public void SetSequence(MelodySequence sequence)
        {
            this.sequence = sequence;
        }
        
        private string SavePath
        {
            get
            {
                return "save" + System.IO.Path.DirectorySeparatorChar + category.CategoryName + System.IO.Path.DirectorySeparatorChar + SAVE_FILE;
            }
        }

        public AccompanimentGenerator(CompositionCategory category, PatchNames instrument)
        {
            this.category = category;
            this.instrument = instrument;
        }

        public AccompanimentGenerator()
        {

        }


        public SampleSet LoadSampleSetFromComposition(Composition comp)
        {
            SampleSet set = new SampleSet();
            

            if (comp.Tracks.Count < 2)
                return set;

            var mainSeq = comp.Tracks[0].GetMainSequence() as MelodySequence;
            var mainNotes = mainSeq.ToArray();


            for (int track = 1; track < comp.Tracks.Count; track++)
            {
                Dictionary<Note, int> frequencies = new Dictionary<Note, int>();

                if (comp.Tracks[track].Instrument != instrument)
                {
                   // Console.WriteLine("\tSkipping instrument {0}", comp.Tracks[track].Instrument);
                    continue;
                }

               

                var seq = comp.Tracks[track].GetMainSequence() as MelodySequence;
                var notes = seq.ToArray();

                var max = Math.Min(mainNotes.Length, notes.Length);

                if(seq.TotalNoteDuration() < seq.TotalRestDuration())
                {
                    continue;
                }

                Console.WriteLine("\tAdding instrument {0}", comp.Tracks[track].Instrument);

                for (int j = 0; j < max; j++ )
                {
                    if (!frequencies.ContainsKey(notes[j]))
                        frequencies[notes[j]] = 1;
                    else frequencies[notes[j]] += 1;
                }

               /* // Filtering
                for (int j = 0; j < max; j++)
                {
                    double normalizedFrequency = frequencies[notes[j]] / (double)max;
              //      if (normalizedFrequency < filterThreshold)
              //          continue;

                    if (notes[j].Velocity > 0)
                        notes[j].Velocity = 127;

                    if (!noteHashes.ContainsKey(notes[j]))
                        noteHashes[notes[j]] = hash++;
                }*/
                

                int mainTrackTime = 0;
                int accompTrackTime = 0;
                int incr = 0;
                for (int j = 0; j < max; j++)
                {
                    // make sure to use closest note
                    if (j + incr >= max)
                        break;
                 /*   mainTrackTime += mainNotes[j].Duration;
                    accompTrackTime += notes[j + incr].Duration;
                    while(accompTrackTime < mainTrackTime)
                    {
                        incr++;             
                        if (j + incr + 1 > max)
                            break;
                        accompTrackTime += notes[j + incr].Duration;
                    }*/


                    if (j + incr > max - 1)
                        break;

/*                    if (!noteHashes.ContainsKey(notes[j + incr]))
                        continue;

                    // caching notes
                    if (noteHashes[notes[j + incr]] > MAX_OUTPUTS - 1)
                        continue;
                    */

                    Sample s = GetSample(mainNotes[j],notes[j+incr]);

                    set.add(s);
                }

            }

            return set;
        }

        public Sample GetSample(Note input, Note output)
        {
          /*  var outputs = new double[MAX_OUTPUTS];
            
            if(output != null)
                outputs[noteHashes[output]] = 1;*/


            var outputs = new double[]{0,0};
            if (output != null)
                outputs = new double[] { (double)output.NotePitch / 12.0, (double)output.Duration / 64.0 };
            Sample s = new Sample(new double[] { (double)input.NotePitch / 12.0, (double)input.Duration / 64.0 },
               outputs);

            return s;
        }

        public SampleSet LoadSampleSetFromMelody(string path)
        {
            var comp = Composition.LoadFromMIDI(path);
            return LoadSampleSetFromComposition(comp);
        }

        public SampleSet GenerateSamples(Composition[] compositions)
        {
            SampleSet set = new SampleSet();
            foreach(var comp in compositions)
            {
                SampleSet songset = new SampleSet();
                //try
               // {
                    songset = LoadSampleSetFromComposition(comp);
               // }
              //  catch (Exception e)
              //  {
              //      Console.WriteLine(e.Message);
              //  }
                set.addAll(songset);
            }
            return set;
        }

        public SampleSet generateSamples(string path)
        {

            SampleSet set = new SampleSet();
          

            var files = Utils.GetFiles(path);
            int percentage = files.Length / 100;
            Parallel.For(0, files.Length, j =>
            {
                var f = files[j];
                bool continue_ = true;
                if (System.IO.Path.GetExtension(f) != ".mid")
                    continue_ = false;
                Console.WriteLine("Adding {0}",System.IO.Path.GetFileNameWithoutExtension(f));
                if (continue_)
                {
                    SampleSet songset = new SampleSet();
                    try
                    {
                        songset = LoadSampleSetFromMelody(files[j]);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    set.addAll(songset);
                }
            });

            return set;

        }

        public static Net RNN()
        {
            var gen = new LSTMGenerator();
            gen.inputLayer(MAX_INPUTS);
            gen.hiddenLayer(MAX_HIDDEN, CellType.SIGMOID, CellType.TANH, CellType.TANH, true);
            // gen.hiddenLayer(hidden, CellType.SIGMOID1);

            gen.outputLayer(MAX_OUTPUTS, CellType.TANH);
            Net net = gen.generate();
            return net;
        }

        public Note[] GenerateMelody(MelodySequence inputSeq)
        {
            SampleSet set = new SampleSet();
            var mainNotes = inputSeq.ToArray();

            for (int j = 0; j < mainNotes.Length && j < 200; j++)
            {
                Sample s = GetSample(mainNotes[j],null);

                set.add(s);
            }

            List<Note> notes = new List<Note>();
            var reverseHashes = Utils.ReverseDictionary(noteHashes);
            int mainTime = 0;
            int accompTime = 0;
            foreach (Sample sample in set)
            {
                var res = ComputeNetworkOutput(sample);
                //var maxIndex = GetMaxIndex(res);
                Note outputNote = null;
                if(res[0] <= 0.05)
                    outputNote = new Note(-1, (int)(res[1] * 64.0),0);
                else
                    outputNote = new Note((NoteNames)(res[0] * 12), 4, (Durations)(res[1] * 64.0));
                outputNote.StandardizeDuration();
                notes.Add(outputNote);
             /*   if (reverseHashes.ContainsKey(maxIndex))
                {
                    outputNote = reverseHashes[maxIndex];
                    notes.Add(outputNote);
                    /*accompTime += outputNote.Duration;
                    if (mainTime > accompTime)
                    {
                        notes.Add(new Note(-1, mainTime - accompTime, 0));
                        accompTime += mainTime - accompTime;
                    }
                    mainTime += (int)(sample.getInput()[1] * 64.0);*/
                //}
            }

            return notes.ToArray();

        }

        /// <summary>
        /// Returns the index of the maximum value
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static int GetMaxIndex(double[] values)
        {
            int max_index = 0;
            double max_val = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max_val)
                {
                    max_val = values[i];
                    max_index = i;
                }
            }

            return max_index;
        }

        private double[] ComputeNetworkOutput(Sample sample)
        {
            ann.reset();
            double[] result = new double[ann.getOutputCells()];
            NetTools.performForward(ann, sample);
            ann.output(result, 0);
            return result;
        }

        public void Initialize()
        {
          /*  if (System.IO.File.Exists(SavePath + "1"))
            {
                Console.WriteLine("Loading from file");
                LoadFromFile();
            }
            else*/
            {
                Console.WriteLine("Generating from library");
                TrainNetwork();
            }
        }

        public void TrainNetwork()
        {

            Console.WriteLine("Training Network");

            SampleSet samples = GenerateSamples(category.Compositions);
            
            ann = RNN();

            ann.rebuffer(samples.maxSequenceLength());
            ann.initializeWeights(rnd);

            GradientDescent trainer = new GradientDescent();
            trainer.setNet(ann);
            trainer.setRnd(rnd);
            trainer.setPermute(false);

            trainer.setTrainingSet(samples);

            trainer.setLearningRate(learningrate);
            trainer.setMomentum(momentum);
            trainer.setEpochs(epochs);
  
            trainer.train();

            Save();
        }

        private void Save()
        {
            string root = System.IO.Path.GetDirectoryName(SavePath);
            if (!System.IO.Directory.Exists(root))
                System.IO.Directory.CreateDirectory(root);

            System.IO.FileStream fs = new System.IO.FileStream(SavePath + "1", System.IO.FileMode.Create);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bin = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();            
            bin.Serialize(fs, ann);
            fs.Close();

            fs = new System.IO.FileStream(SavePath + "2", System.IO.FileMode.Create);
            ProtoBuf.Serializer.PrepareSerializer<Dictionary<Note, int>>();
            ProtoBuf.Serializer.Serialize(fs, noteHashes);
            fs.Close();
        }

        public void LoadFromFile()
        {
            System.IO.FileStream fs = new System.IO.FileStream(SavePath + "1", System.IO.FileMode.Open);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bin = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            this.ann = bin.Deserialize(fs) as Net;

            fs = new System.IO.FileStream(SavePath + "2", System.IO.FileMode.Open);
            ProtoBuf.Serializer.PrepareSerializer<Dictionary<Note, int>>();
            this.noteHashes = ProtoBuf.Serializer.Deserialize<Dictionary<Note, int>>(fs);
        
            fs.Close();
        }

        public Representation.MelodySequence Generate()
        {
            return new MelodySequence(GenerateMelody(sequence));
        }

        public Representation.MelodySequence Next()
        {
            return Generate();
        }

        public bool HasNext
        {
            get { return true; }
        }
    }

}
#endif