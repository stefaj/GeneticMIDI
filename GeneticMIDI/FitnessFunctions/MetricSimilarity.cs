using AForge.Genetic;
using GeneticMIDI.Metrics;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.FitnessFunctions
{

    public enum SimilarityType { Cosine, Euclidian, Pearson}
    
    /// <summary>
    /// Computes similarity according to the given metrics against a single melody sequence
    /// </summary>
    public class MetricSimilarity : IFitnessFunction
    {
        IMetric[] metrics;
        Note[] target;

        Dictionary<object, float>[] target_metrics;
        SimilarityType type;
        public MetricSimilarity(MelodySequence seq, IMetric metric, SimilarityType type = SimilarityType.Cosine)
        {
            this.metrics = new IMetric[]{metric};
            this.target = seq.ToArray();
            this.type = type;
            this.target_metrics = new Dictionary<object, float>[] { this.metrics[0].Generate(this.target) };
           
        }

        public MetricSimilarity(MelodySequence seq, IMetric[] metrics, SimilarityType type = SimilarityType.Cosine)
        {
            this.metrics = metrics;
            this.target = seq.ToArray();
            target_metrics = new Dictionary<object, float>[metrics.Length];
            this.type = type;
            for (int i = 0; i < metrics.Length; i++)
                target_metrics[i] = this.metrics[i].Generate(this.target);
        }


        public static MetricSimilarity GenerateMetricSimilarityMulti(IEnumerable<Composition> comps, IMetric[] metrics, SimilarityType type = SimilarityType.Cosine)
        {
            List<Note> notes = new List<Note>();
            foreach(var comp in comps)
            {
                var seq = comp.Tracks[0].GetMainSequence() as MelodySequence;
                foreach (var n in seq.Notes)
                    notes.Add(n);
            }

            return new MetricSimilarity(new MelodySequence(notes), metrics, type);
        }

        public double Evaluate(IChromosome chromosome)
        {
            var chromo = chromosome as GPCustomTree;
            if (chromo == null)
                return 0;
            var notes = chromo.GenerateNotes();

            if (type == SimilarityType.Cosine)
                return ComputeFitness(notes.ToArray());
            if (type == SimilarityType.Euclidian)
                return ComputeFitness2(notes.ToArray(), new AForge.Math.Metrics.EuclideanSimilarity());
            if (type == SimilarityType.Pearson)
                return ComputeFitness2(notes.ToArray(), new AForge.Math.Metrics.PearsonCorrelation());
            else
                return ComputeFitness(notes.ToArray()); 
        }

        public float ComputeFitnessCorrelation(Note[] individual)
        {
            int max_length = Math.Max(individual.Length, target.Length);
            int min_length = Math.Min(individual.Length, target.Length);
            double[] pitches1 = new double[max_length];
            double[] durations1 = new double[max_length];
            double[] pitches2 = new double[max_length];
            double[] durations2 = new double[max_length];
            for(int i = 0; i < max_length; i++)
            {
                pitches1[i] = 0; pitches2[i] = 0;
                durations1[i] = 0; durations2[i] = 0;

                if(i < individual.Length)
                {
                    pitches1[i] = individual[i].Pitch;
                    durations1[i] = individual[i].Duration;
                }
                if(i < target.Length)
                {
                    pitches2[i] = target[i].Pitch;
                    durations2[i] = target[i].Duration;
                }
            }
            float diff = Math.Abs(individual.Length - target.Length);
            if (diff < 1)
                diff = 1;
            var metr = new AForge.Math.Metrics.EuclideanSimilarity();
            return (float)(metr.GetSimilarityScore(pitches1,pitches2) + metr.GetSimilarityScore(durations1, durations2)) ;
        }

        public float ComputeFitness2(Note[] individual, AForge.Math.Metrics.ISimilarity similarity)
        {
            float weighted = 0;
             for (int i = 0; i < metrics.Length; i++)
            {
                var x = metrics[i].Generate(individual);

                List<double> arr1 = new List<double>(individual.Length);
                List<double> arr2 = new List<double>(individual.Length);

                foreach (var k in target_metrics[i].Keys)
                 {
                     if (!x.ContainsKey(k))
                     {
                         arr1.Add(0);
                         arr2.Add(target_metrics[i][k]);
                         continue;
                     }
                     
                     arr1.Add(x[k]);
                     arr2.Add(target_metrics[i][k]);
                 }
                weighted += (float)similarity.GetSimilarityScore(arr1.ToArray(), arr2.ToArray());
            }

             return weighted / (float)metrics.Length;
        }

        public float ComputeFitness(Note[] individual)
        {
            float weighted_sum = 0;
            for (int i = 0; i < metrics.Length; i++)
            {
                var x = metrics[i].Generate(individual);
                var t = target_metrics[i];
                float magx = 0;
                float magt = 0;
                float dot = 0;
                foreach(var v in x.Values)
                    magx += v * v;
                foreach(var v in t.Values)
                    magt += v * v; 
                foreach (var p in x.Keys)
                {
                    float f1 = x[p];
                    if (!t.ContainsKey(p))
                        continue;

                    float f2 = t[p];
                    dot += f1 * f2;
                }
                magx = (float)Math.Sqrt(magx);
                magt = (float)Math.Sqrt(magt);
                if (magx == 0 || magt == 0)
                    return 0;
                weighted_sum += dot / (magx * magt);
            }
            return weighted_sum / metrics.Length;
        }
    }
}
