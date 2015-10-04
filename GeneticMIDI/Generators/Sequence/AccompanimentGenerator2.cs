using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using AForge.Neuro;
using AForge.Neuro.Learning;
using GeneticMIDI.ML;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    public class AccompanimentGenerator2
    {
        PatchNames instrument;
        ActivationNetwork network;
        Dictionary<Note, int> noteHashes = new Dictionary<Note, int>();
        int hash;
        const int INPUT_NOTES = 1;
        const int MAX_INPUTS = INPUT_NOTES * 2;
        const int MAX_OUTPUTS = 2;
        MelodySequence sequence;
        CompositionCategory category;


        public AccompanimentGenerator2(CompositionCategory category, PatchNames instrument)
        {
            this.category = category;
            this.instrument = instrument;
        }


        public void Train()
        {
            var samples = GenerateSamples(category.Compositions);
            double[][] inputs = new double[samples.Length][];
            double[][] outputs = new double[samples.Length][];
            for (int i = 0; i < samples.Length; i++)
            {
                inputs[i] = samples[i].Inputs;
                outputs[i] = samples[i].Outputs;
            }
            // Create a Bernoulli activation function
            //var function = new BernoulliFunction(alpha: 0.5);
            var function = new SigmoidFunction(2);
            // Create a Restricted Boltzmann Machine for 6 inputs and with 1 hidden neuron
            //network = new RestrictedBoltzmannMachine(function, inputsCount: MAX_INPUTS, hiddenNeurons: MAX_OUTPUTS);
            network = new ActivationNetwork(function, MAX_INPUTS, 11, MAX_OUTPUTS); 

            // Create the learning algorithm for RBMs
        /*    var teacher = new ContrastiveDivergenceLearning(network)
            {
                Momentum = 0.1,
                LearningRate = 0.02
            };*/



            // create neural network
         /*     network = new ActivationNetwork(
                new SigmoidFunction( 2 ),
                2, // two inputs in the network
                10, // two neurons in the first layer
                2 ); // one neuron in the second layer*/


            var teacher = new ResilientBackpropagationLearning(network);

            // learn 5000 iterations
            int epochs = 50000;
            for (int i = 0; i < epochs; i++)
            {
           
                var e = teacher.RunEpoch(inputs,outputs);

                Console.WriteLine("{0} : {1}", i / (double)epochs * 100, e);


            }

        }

        public void SetSequence(MelodySequence sequence)
        {
            this.sequence = sequence;
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

                if (seq.TotalNoteDuration() < seq.TotalRestDuration())
                {
                    continue;
                }

                Console.WriteLine("\tAdding instrument {0}", comp.Tracks[track].Instrument);

                for (int j = 0; j < max; j++)
                {
                    if (notes[j].Velocity > 0)
                        notes[j].Velocity = 127;

                    notes[j].StandardizeDuration();

                    if (!frequencies.ContainsKey(notes[j]))
                        frequencies[notes[j]] = 1;
                    else frequencies[notes[j]] += 1;
                }

                // Filtering
                for (int j = 0; j < max; j++)
                {
                    double normalizedFrequency = frequencies[notes[j]] / (double)max;
                    //      if (normalizedFrequency < filterThreshold)
                    //          continue;

                    if (!noteHashes.ContainsKey(notes[j]))
                        noteHashes[notes[j]] = hash++;
                }


                int mainTime = 0;
                int accompTime = 0;
                int mainIndex = 0; int mainOff = 0;
                int accompIndex = 0; int accompOff = 0;
                const int GroupSize = 20;
                Note[] prevAccompanyNotesGroup = null;
                for (int j = GroupSize*2; mainIndex < max && accompIndex < max; j += GroupSize)
                {
                    mainIndex = j + mainOff;
                    accompIndex = j + accompOff;

                    if (mainIndex > max - 1 || accompIndex > max - 1)
                        break;


                    /*if (noteHashes[notes[j]] > MAX_OUTPUTS - 1)
                        continue;*/


                    Note[] accompanyNotesGroup = new Note[GroupSize];
                    Note[] mainNotesGroup = new Note[GroupSize];
                    for (int k = 0; k < GroupSize; k++)
                    {
                        accompanyNotesGroup[GroupSize - k - 1] = notes[accompIndex - k];
                        mainNotesGroup[GroupSize - k - 1] = mainNotes[mainIndex - k];
                    }
                    for (int k = 0; k < GroupSize; k++)
                    {
                        if (!noteHashes.ContainsKey(mainNotesGroup[k]) || !noteHashes.ContainsKey(accompanyNotesGroup[k]))
                            continue;
                        Note[] mainPrev = new Note[INPUT_NOTES];
                        for(int l = 0; l < INPUT_NOTES; l++)
                        {
                            mainPrev[l] = notes[accompIndex - k - l];
                        }
                        Note accomp = notes[accompIndex - k];
                        Sample s = GetSample(mainPrev, accomp);
                        set.Add(s);
                        
                    }

                    mainTime += mainNotes[mainIndex].Duration;
                    accompTime += notes[accompIndex].Duration;

                    // Equalize every GroupSize notes
                    while (accompTime < mainTime)
                    {
                        accompIndex = j + (++accompOff);
                        if (accompIndex >= notes.Length - 1)
                            break;
                        accompTime += notes[accompIndex].Duration;
                    }
                }

            }

            return set;
        }

        public double[] GetNoteRepresentation(Note n)
        {
            if (n == null)
                return new double[] { 0, 0 };
            return new double[] { (double)n.Pitch / 128.0, (double)n.Duration / 64.0 };
        }


        public Sample GetSample(Note input, Note output)
        {

            Sample s = new Sample(GetNoteRepresentation(input), GetNoteRepresentation(output));

            return s;
        }

        public Sample GetSample(Note[] input, Note output)
        {
            List<double> inputRep = new List<double>();
            foreach (var n in input)
                inputRep.AddRange(GetNoteRepresentation(n));

            Sample s = new Sample(inputRep.ToArray(), GetNoteRepresentation(output));

            return s;
        }

        public SampleSet GenerateSamples(Composition[] compositions)
        {
            SampleSet set = new SampleSet();
            foreach (var comp in compositions)
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
                set.AddAll(songset);
            }
            return set;
        }

        public Note[] GenerateMelody(MelodySequence inputSeq)
        {
            SampleSet set = new SampleSet();
            var mainNotes = inputSeq.ToArray();

            for (int j = INPUT_NOTES; j < mainNotes.Length && j < 200; j++)
            {
                Note[] prevNotes = new Note[INPUT_NOTES];
                for(int k = 0; k < INPUT_NOTES; k++)
                {
                    prevNotes[INPUT_NOTES - k-1] = mainNotes[j - k];
                }
                Sample s = GetSample(prevNotes, null);

                set.Add(s);
            }

            List<Note> notes = new List<Note>();
            var reverseHashes = Utils.ReverseDictionary(noteHashes);
            int mainTime = 0;
            int accompTime = 0;

            int i = 0;
            foreach (Sample sample in set)
            {

                var res = network.Compute(sample.Inputs);
                //Note n = new Note((NoteNames)((int)(res[0] * 12)), 6, (Durations)((int)(res[1] * 64.0)));
                Note n = new Note((int)(res[0] * 128), (int)(res[1] * 64.0));
                n.Duration *= 2;
                n.StandardizeDuration();
                if (mainTime >= accompTime)
                {
                    notes.Add(n);
                    accompTime += (int)n.Duration;
                }

                if(i++ % 20 == 0)
                {
                    notes.Add(new Note(-1, mainTime - accompTime));
                }
                mainTime += (int)(sample.Inputs[1] * 64);
               
                
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



        public Representation.MelodySequence Generate()
        {
            return new MelodySequence(GenerateMelody(sequence));
        }


    }
}
