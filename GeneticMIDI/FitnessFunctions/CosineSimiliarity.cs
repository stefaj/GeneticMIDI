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
    class CosineSimiliarity : IFitnessFunction
    {
        IMetric metric;
        Note[] target;

        Dictionary<Pair, int> target_metric;

        public CosineSimiliarity(string target, IMetric metric)
        {
            this.metric = metric;
            this.target = Note.LoadFromFileSampled(target);
            this.target_metric = this.metric.Generate(this.target);
        }
        public double Evaluate(IChromosome chromosome)
        {
            var chromo = chromosome as GPCustomTree;
            if (chromo == null)
                return 0;
            var notes = chromo.GenerateNotes();


            return ComputeFitness(notes.ToArray());
        }

        public float ComputeFitness(Note[] individual)
        {
            var x = metric.Generate(individual);
            var t = target_metric;
            float magx = 0;
            foreach(var v in x.Values)
            {
                magx += v * v;
            }
            magx = (float)Math.Sqrt(magx);
            float magt = 0;
            foreach(var v in t.Values)
            {
                magt += v * v;
            }
            magt = (float)Math.Sqrt(magt);
            float dot = 0;
            foreach(var p in x.Keys)
            {
                int f1 = x[p];
                if (!t.ContainsKey(p))
                    continue;
                int f2 = t[p];
                dot += f1 * f2;
            }
            if (magx == 0 || magt == 0)
                return 0;
            return dot / (magx * magt);
        }
    }
}
