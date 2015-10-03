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
            List<int[]> inputSequences = new List<int[]>();
            List<int[]> outputSequences = new List<int[]>();
            foreach(Composition comp in cat.Compositions)
            {
                if (comp.Tracks.Count < 2)
                    continue;

                var melInput = comp.Tracks[0].GetMainSequence() as MelodySequence; melInput.Trim(100); melInput.NormalizeNotes(4);
                var melOutput = comp.Tracks[1].GetMainSequence() as MelodySequence; melOutput.Trim(100); melOutput.NormalizeNotes(4);
                if (melInput.Length > melOutput.Length)
                    melInput.Trim(melOutput.Length);
                else if (melOutput.Length > melInput.Length)
                    melOutput.Trim(melInput.Length);

                book.Add(melInput.Notes); book.Add(melOutput.Notes);
                inputSequences.Add(book.ToCodes(melInput.ToArray()));
                outputSequences.Add(book.ToCodes(melOutput.ToArray()));
            }

            if (outputSequences.Count != inputSequences.Count)
                throw new Exception("MSP");
            for(int i = 0; i < outputSequences.Count; i++)
            {
                if (outputSequences[i].Length != inputSequences[i].Length)
                    throw new Exception("MSP 2");
            }

            var topology = new Forward(states: 50);

            hmm = new HiddenMarkovModel(20, book.TotalUniqueSymbols + 1);
            var teacher = new Accord.Statistics.Models.Markov.Learning.MaximumLikelihoodLearning(hmm) {UseLaplaceRule=false /*Tolerance = 0.1, Iterations=0*/};
            //var teacher = new ViterbiLearning(hmm);
            
                double ll = teacher.Run(outputSequences.ToArray(), inputSequences.ToArray());
                Console.WriteLine("Error: {0}", ll);
 
        }
    }
}
