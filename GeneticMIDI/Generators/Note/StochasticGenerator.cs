using Accord.MachineLearning.Bayes;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using GeneticMIDI.Metrics.Frequency;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators
{
    public class StochasticGenerator : IGenerator
    {
        HiddenMarkovModel pitch_hmm;
        Dictionary<int, int> notes_map;

        MelodySequence base_seq = null;

        public event OnFitnessUpdate OnPercentage;

        public int MaxGenerations { get; set; }

        public StochasticGenerator(MelodySequence[] seqs)
        {
            if(seqs != null && seqs.Length > 0)
                this.base_seq = seqs[0];

            int count = seqs.Length;

            List<int[]> notes = new List<int[]>();

            List<int[]> bayesInputs = new List<int[]>();
            List<int> bayesOutputs = new List<int>();
            notes_map = new Dictionary<int, int>();

            int max_notes = 0;
          
            int note = 0;
            foreach (MelodySequence m in seqs)
            {
                Note[] song = m.ToArray();

                int[] _notes = new int[song.Length];
                int[] _durations = new int[20];

                for (int i = 0; i < song.Length; i++)
                {
                    var unote = song[i].Pitch + song[i].Duration*128;
                    if (notes_map.ContainsKey(unote))
                        _notes[i] = notes_map[unote];
                    else
                    {
                        notes_map[unote] = note++;
                        _notes[i] = notes_map[unote];
                    }

                }
                notes.Add(_notes);
                
            }
            max_notes = note;
            Console.WriteLine("Training Pitches");
            pitch_hmm = new HiddenMarkovModel(50, max_notes);
            var teacher = new BaumWelchLearning(pitch_hmm) { Tolerance = 0.0001, Iterations = 0 };
            var __pitches = notes.ToArray();
            teacher.Run(__pitches);
            teacher.Run(__pitches);   
            Console.WriteLine("Done training");

            this.MaxGenerations = 2000;

        }
        

        public IEnumerable<Note> Generate()
        {
            if (base_seq == null)
                return GenerateSingle();

            float best_fitness = 0;
            IEnumerable<Note> best_individual = null;

            FitnessFunctions.MetricSimilarity sim = new 
                FitnessFunctions.MetricSimilarity(base_seq, new Metrics.IMetric[] {new RhythmicInterval(), new Rhythm(), new RhythmicBigram()/*,new MelodicBigram()*/ }, FitnessFunctions.SimilarityType.Euclidian);

            int percentage = MaxGenerations / 100;
            for(int i = 0; i < MaxGenerations; i++)
            {
                var indiv = GenerateSingle();
                float fitness = sim.ComputeFitness(indiv);
                if(fitness > best_fitness)
                {
                    best_fitness = fitness;
                    best_individual = indiv;
                }

                if (i > percentage)
                {
                    if (OnPercentage != null)
                        OnPercentage(this, i, best_fitness);
                    percentage += MaxGenerations / 100;
                }
            }
            return best_individual;
        }

        public Note[] GenerateSingle()
        {
            Dictionary<int, int> reverse_note_map = new Dictionary<int, int>();
            foreach (int k in notes_map.Keys)
                reverse_note_map[notes_map[k]] = k;

            var notes1 = pitch_hmm.Generate(150);

            Note[] notes3 = new Note[notes1.Length];
            for (int i = 0; i < notes1.Length; i++)
            {

                var n = reverse_note_map[notes1[i]];
                int pitch = n % 128;
                int duration = n / 128;
                //duration = (int)Durations.qn;
                notes3[i] = new Note(pitch, duration);
            }
            return notes3;

        }
    }
}
