using Accord.Statistics.Filters;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;
using DotNetLearn.Data;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    public class HMMGenerator
    {
       
        PatchNames instrument;
        HiddenMarkovModel hmm;
        Codebook<Note> book;
        public HMMGenerator(PatchNames instrument)
        {
            this.book = new Codebook<Note>();
            this.instrument = instrument;













            Accord.Math.Tools.SetupGenerator(10);

            // Consider some phrases:
            // 
            string[][] phrases =
{
    "The Big Brown Fox Jumps Over the Ugly Dog".Split(new char[]{' '},  StringSplitOptions.RemoveEmptyEntries),
    "This is too hot to handle".Split(new char[]{' '},  StringSplitOptions.RemoveEmptyEntries),
    "I am flying away like a gold eagle".Split(new char[]{' '},  StringSplitOptions.RemoveEmptyEntries),
    "Onamae wa nan desu ka".Split(new char[]{' '},  StringSplitOptions.RemoveEmptyEntries),
    "And then she asked, why is it so small?".Split(new char[]{' '},  StringSplitOptions.RemoveEmptyEntries),
    "Great stuff John! Now you will surely be promoted".Split(new char[]{' '},  StringSplitOptions.RemoveEmptyEntries),
    "Jayne was taken aback when she found out her son was gay".Split(new char[]{' '},  StringSplitOptions.RemoveEmptyEntries),
};

            // Let's begin by transforming them to sequence of
            // integer labels using a codification codebook:
            var codebook = new Codification("Words", phrases);

            // Now we can create the training data for the models:
            int[][] sequence = codebook.Translate("Words", phrases);

            // To create the models, we will specify a forward topology,
            // as the sequences have definite start and ending points.
            // 
            var topology = new Forward(states: codebook["Words"].Symbols);
            int symbols = codebook["Words"].Symbols; // We have 7 different words

            // Create the hidden Markov model
            HiddenMarkovModel hmm = new HiddenMarkovModel(topology, symbols);

            // Create the learning algorithm
            var teacher = new ViterbiLearning(hmm);

            // Teach the model about the phrases
            double error = teacher.Run(sequence);

            // Now, we can ask the model to generate new samples
            // from the word distributions it has just learned:
            // 
            List<int> sample = new List<int>();
            int count = 10;
            sample.Add(hmm.Generate(1)[0]);
            while(sample.Count < count)
            {
                var k = hmm.Predict(sample.ToArray(), 1);
                sample.AddRange(k);
            }

            // And the result will be: "those", "are", "words".
            string[] result = codebook.Translate("Words", sample.ToArray());
        }

        public MelodySequence Generate(MelodySequence seq)
        {
            MelodySequence seqOut = new MelodySequence();
            
            var hashedNotes = hmm.Generate(100);
            //double trouble = 0;
            //var hashedNotes = hmm.Decode(GetHashes(seq.ToArray()), out trouble);
            seqOut.AddNotes(book.Translate(hashedNotes));

            return seqOut;
        }


        public void Train(CompositionCategory cat)
        {
            Accord.Math.Tools.SetupGenerator(42);    
            List<int[]> sequences = new List<int[]>();
            foreach(Composition comp in cat.Compositions)
            {
                foreach(var track in comp.Tracks)
                {
                    if (track.Instrument != instrument)
                        continue;
                    var mel = track.GetMainSequence() as MelodySequence;

                    book.Add(mel.Notes);
                    sequences.Add(book.ToCodes(mel.ToArray()));
                    break;
                }
                
            }

            var topology = new Ergodic(states: 10);

            hmm = new HiddenMarkovModel(topology, book.TotalUniqueSymbols + 1);
            var teacher = new BaumWelchLearning(hmm) { Tolerance = 0.001, Iterations = 0 };
            //var teacher = new ViterbiLearning(hmm);

            double ll = teacher.Run(sequences.ToArray());
        }
    }
}
