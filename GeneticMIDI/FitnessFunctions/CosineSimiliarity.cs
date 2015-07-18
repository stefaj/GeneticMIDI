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
    public class CosineSimiliarity : IFitnessFunction
    {
        IMetric[] metrics;
        Note[] target;

        Dictionary<object, float>[] target_metrics;

        public CosineSimiliarity(MelodySequence seq, IMetric metric)
        {
            this.metrics = new IMetric[]{metric};
            this.target = seq.ToArray();
            this.target_metrics = new Dictionary<object, float>[] { this.metrics[0].Generate(this.target) };
        }

        public CosineSimiliarity(MelodySequence seq, IMetric[] metrics)
        {
            this.metrics = metrics;
            this.target = seq.ToArray();
            target_metrics = new Dictionary<object, float>[metrics.Length];
            for (int i = 0; i < metrics.Length; i++)
                target_metrics[i] = this.metrics[i].Generate(this.target);
        }

        public double Evaluate(IChromosome chromosome)
        {
            var chromo = chromosome as GPCustomTree;
            if (chromo == null)
                return 0;
            var notes = chromo.GenerateNotes();


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

        public float ComputeFitness2(Note[] individual)
        {
            float weighted = 0;
             var cos = new AForge.Math.Metrics.CosineSimilarity();
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
                 weighted += (float)cos.GetSimilarityScore(arr1.ToArray(), arr2.ToArray());
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
