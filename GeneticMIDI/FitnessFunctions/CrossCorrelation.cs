using AForge.Genetic;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.FitnessFunctions
{
    class CrossCorrelation : IFitnessFunction 
    {
        Note[] target;
        double[] pitches;
        double[] durations;
        double highest_peak;
        public CrossCorrelation(MelodySequence seq)
        {
            this.target = seq.ToArray();
            pitches = new double[this.target.Length];
            durations = new double[this.target.Length];
            for(int i = 0; i < this.target.Length; i++)
            {
                pitches[i] = this.target[i].Pitch;
                durations[i] = this.target[i].Duration;
            }
            highest_peak = GetSimilarity(pitches, pitches, durations, durations);
        }

        private double GetSimilarity(double[] pitches1, double[] pitches2, double[] durations1, double[] durations2)
        {
            double peak;
            double[] ou;
            alglib.corrr1dcircular(pitches1, pitches1.Length, pitches2, pitches2.Length, out ou);
            double max = 0;
            for(int i =0; i < ou.Length; i++)
            {
                if(ou[i] > max)
                {
                    max = ou[i];
                }
            }
            peak = max;
            alglib.corrr1dcircular(durations1, durations1.Length, durations2, durations2.Length, out ou);
            max = 0;
            for(int i =0; i < ou.Length; i++)
            {
                if(ou[i] > max)
                {
                    max = ou[i];
                }
            }
            double dur1=0;
            for (int i = 0; i < durations1.Length; i++)
                dur1 += durations1[i];

            double dur2 = 0;
            for (int i = 0; i < durations2.Length; i++)
                dur2 += durations2[i];

            double diff = (dur2 - dur1) + (durations2.Length - durations1.Length);
            diff = diff * diff;

            peak += max - diff ;
            if (peak < 0)
                return 0;
            return Math.Sqrt(peak);

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
           double[] target_pitches = new double[individual.Length];
           double[] target_durations = new double[individual.Length];
           for (int i = 0; i < individual.Length; i++)
           {
               target_pitches[i] = individual[i].Pitch;
               target_durations[i] = individual[i].Duration;
           }
           return (float)(GetSimilarity(pitches, target_pitches, durations, target_durations) / highest_peak);
       }
    }
}
