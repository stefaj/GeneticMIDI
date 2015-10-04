using AForge.Genetic;
using GeneticMIDI.Generators.NoteGenerators;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators
{
    public delegate void OnFitnessUpdate(object sender, int percentage, double fitness);

    public class GeneticGenerator : INoteGenerator, IPlaybackGenerator
    {

        public int MaxGenerations { get; set; }

        IFitnessFunction fitnessFunction;


        Population pop = null;

        public event OnFitnessUpdate OnPercentage;

        CompositionCategory cat;
        int avgLength = 0;

        public GeneticGenerator(IFitnessFunction fitnessFunction, CompositionCategory cat=null)
        {
            this.fitnessFunction = fitnessFunction;
            this.cat = cat;
            this.MaxGenerations = 1000;

            if (cat != null)
            {
                // Markov generator
                var mark = new MarkovChainGenerator(2);

                // Allowed notes
                HashSet<Durations> durs = new HashSet<Durations>();
                HashSet<int> fullPitches = new HashSet<int>();

                foreach(var c in cat.Compositions)
                {
                    var cloneMel = (c.Tracks[0].GetMainSequence() as MelodySequence).Clone() as MelodySequence;
                    
                    cloneMel.StandardizeDuration();
                    mark.AddMelody(cloneMel);

                    foreach (var n in cloneMel.Notes)
                    {
                        durs.Add(NoteGene.GetClosestDuration(n.Duration));
                        fullPitches.Add(n.Pitch);
                    }

                    avgLength += cloneMel.Length;
                }
                avgLength /= cat.Compositions.Length;
                GPCustomTree.generator = mark;

                NoteGene.AllowedDurations = durs.ToArray();
                NoteGene.AllowedFullPitches = fullPitches.ToArray();
               
            }
        }



        public MelodySequence Generate()
        {
            NoteGene baseGene = new NoteGene(GPGeneType.Function);
            baseGene.Function = NoteGene.FunctionTypes.Concatenation;
            GPCustomTree tree = new GPCustomTree(baseGene);
            if (cat != null)
            {
       
                int length = avgLength;
                int depth = (int)Math.Ceiling(Math.Log(length, 2));
                GPCustomTree.MaxInitialLevel = depth - 2;
                GPCustomTree.MaxLevel = depth + 3;
            }
            var selection = new EliteSelection();
            pop = new Population(30, tree, fitnessFunction, selection);
            
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
            return FixMelodySequence(new MelodySequence(notes));
        }

        private MelodySequence FixMelodySequence(MelodySequence seq)
        {
            var newNotes = new List<Note>();
            var notes = seq.ToArray();
            if (notes.Length > 1)
                newNotes.Add(notes[0]);
            for(int i = 1; i < notes.Length; i++)
            {
                bool add = true;

                var note = notes[i];
                var prevNote = notes[i - 1];

                if (!note.IsRest())
                {

                    if (Math.Abs(note.Pitch - prevNote.Pitch) > 24)
                    {
                        add = false;
                    }
                }

                if (add)
                    newNotes.Add(note);
            }
            return new MelodySequence(newNotes.ToArray());
        }

        public MelodySequence Next()
        {
            if (pop == null)
                throw new Exception("Run generate first");

            GPCustomTree.generator = null;
            pop.BestChromosome.Crossover(pop.BestChromosome);
            pop.BestChromosome.Mutate();

            pop.Shuffle();

            for (int i = 0; i < 10; i++)
                pop.RunEpoch();
            if (OnPercentage != null)
                OnPercentage(this, MaxGenerations++, pop.FitnessAvg);
            
            GPCustomTree best = pop.BestChromosome as GPCustomTree;
            var notes = best.GenerateNotes();
            return FixMelodySequence(new MelodySequence(notes));
        }


        public bool HasNext
        {
            get { return pop != null; }
        }

        IPlayable IPlaybackGenerator.Generate()
        {
            return Generate();
        }

        IPlayable IPlaybackGenerator.Next()
        {
            return Next();
        }
    }
}
