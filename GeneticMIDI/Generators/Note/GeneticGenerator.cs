using AForge.Genetic;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators
{
    public delegate void OnFitnessUpdate(object sender, int percentage, double fitness);

    public class GeneticGenerator : IGenerator
    {

        public int MaxGenerations { get; set; }

        IFitnessFunction fitnessFunction;
        MelodySequence base_seq = null;

        public event OnFitnessUpdate OnPercentage;

        public GeneticGenerator(IFitnessFunction fitnessFunction, MelodySequence base_seq = null)
        {
            this.fitnessFunction = fitnessFunction;
            this.base_seq = base_seq;
            this.MaxGenerations = 2000;

            if (base_seq != null)
                CreateUniques();
        }

        private void CreateUniques()
        {
            HashSet<Durations> durs = new HashSet<Durations>();
            HashSet<int> fullPitches = new HashSet<int>();
            foreach(Note n in base_seq.ToArray())
            {
                durs.Add(NoteGene.GetClosestDuration(n.Duration));
                fullPitches.Add(n.Pitch);
            }

            NoteGene.AllowedDurations = durs.ToArray();
            NoteGene.AllowedFullPitches = fullPitches.ToArray();
        }

        public IEnumerable<Note> Generate()
        {
            NoteGene baseGene = new NoteGene(GPGeneType.Function);
            baseGene.Function = NoteGene.FunctionTypes.Concatenation;
            GPCustomTree tree = new GPCustomTree(baseGene);
            if (base_seq != null)
            {
                tree.Generate(base_seq.ToArray());
                tree.Mutate();
                tree.Crossover(tree);
       
                int length = base_seq.Length;
                int depth = (int)Math.Ceiling(Math.Log(length, 2));
                GPCustomTree.MaxInitialLevel = depth - 2;
                GPCustomTree.MaxLevel = depth + 5;
            }
            var selection = new EliteSelection();
            Population pop = new Population(30, tree, fitnessFunction, selection);
            
            pop.AutoShuffling = true;
            pop.CrossoverRate = 0.9;
            pop.MutationRate = 0.1;

            int percentage = MaxGenerations / 100;

            for (int i = 0; i < MaxGenerations; i++)
            {
                if(i>percentage)
                {
                    if (OnPercentage != null)
                        OnPercentage(this, i, pop.FitnessAvg);
                    percentage += MaxGenerations / 100;
                }
               
                pop.RunEpoch();
                if ((int)(i) % 100 == 0)
                    Console.WriteLine(i / (float)MaxGenerations * 100 + "% : " + pop.FitnessAvg);
            }
            
            GPCustomTree best = pop.BestChromosome as GPCustomTree;
            var notes = best.GenerateNotes();
            return notes;
        }

    }
}
