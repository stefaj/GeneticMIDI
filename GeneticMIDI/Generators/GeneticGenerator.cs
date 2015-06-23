using AForge.Genetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators
{
    class GeneticGenerator : IGenerator
    {
        IFitnessFunction fitnessFunction;
        public GeneticGenerator(IFitnessFunction fitnessFunction)
        {
            this.fitnessFunction = fitnessFunction;
        }

        public IEnumerable<Note> Generate()
        {
            NoteGene baseGene = new NoteGene(GPGeneType.Function);
            baseGene.Function = NoteGene.FunctionTypes.Series;
            NoteTree tree = new NoteTree(baseGene);
            var selection = new EliteSelection();
            Population pop = new Population(30, tree, fitnessFunction, selection);
            
            pop.AutoShuffling = true;
            pop.CrossoverRate = 0.8;
            pop.MutationRate = 0.2;

            const int MAX = 2000;
            for (int i = 0; i < MAX; i++)
            {
               
                pop.RunEpoch();
                if ((int)(i) % 100 == 0)
                    Console.WriteLine(i / (float)MAX * 100 + "% : " + pop.FitnessAvg);
            }
            
            GPCustomTree best = pop.BestChromosome as GPCustomTree;
            var notes = best.GenerateNotes();
             return notes;
        }

    }
}
