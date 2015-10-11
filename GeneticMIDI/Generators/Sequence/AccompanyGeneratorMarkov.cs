using DotNetLearn.Markov;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    public class AccompanyGeneratorMarkov : INoteGenerator
    {
        const int ORDER = 2;
        const int SYNC_AFTER_NOTES = 20;
        MarkovTable<Note> table;
        MelodySequence sequence;
        public AccompanyGeneratorMarkov(CompositionCategory cat)
        {
            Add(cat);
        }

        MelodySequence NormalizeSequence(MelodySequence seq)
        {
            MelodySequence normalized = new MelodySequence();
            foreach(var note in seq)
            {
                Note copy = note.Clone() as Note;
                copy.StandardizeDuration();
                //copy.Octave = 6;
                normalized.AddNote(copy);
            }
            return normalized;
        }

        public void SetSequence(MelodySequence seq)
        {
            this.sequence = seq;
        }

        public MelodySequence Generate(MelodySequence seq, int seed)
        {
            var targetSeq = NormalizeSequence(seq);
            
            MelodySequence outputSequence = new MelodySequence();

            var targetSequences = targetSeq.Split(SYNC_AFTER_NOTES);
            int mainDuration = 0;
            int accompDuration = 0;
            foreach(var mel in targetSequences)
            {
                var outputMel = table.Chain(mel.ToArray(), seed);
                MelodySequence outputSeq = new MelodySequence(outputMel);
                
                while(outputSeq.Duration > mel.Duration)
                {
                    outputSeq.RemoveLastNote();
                }
            //    outputSeq.RemoveLastNote();
                outputSeq.AddPause(mel.Duration - outputSeq.Duration);
             //   outputSeq.RemoveLastNote();

                if (mainDuration > accompDuration)
                    outputSeq.AddPause(mainDuration - accompDuration);
                
                outputSequence.AddMelodySequence(outputSeq);

                mainDuration += mel.Duration;
                accompDuration += outputSeq.Duration;
                
            }
            
            
            return outputSequence;
        }

        void Add(CompositionCategory category)
        {
            table = new MarkovTable<Note>(ORDER);
            foreach (var c in category.Compositions)
            {
                if (c.Tracks.Count < 2)
                    continue;
                var mainSeq = NormalizeSequence((c.Tracks[0].GetMainSequence() as MelodySequence));
               
                for (int i = 1; i < c.Tracks.Count; i++)
                {
                    var accompSeq = NormalizeSequence(c.Tracks[i].GetMainSequence() as MelodySequence);
                    if (accompSeq.TotalRestDuration() > accompSeq.TotalNoteDuration())
                        continue;
                    Console.WriteLine("Adding comp {0}:{1}", c.NameTag,i);
                    table.Add(mainSeq.ToArray(), accompSeq.ToArray());
                   // Console.WriteLine("Adding track");
                }

            }
        }


        public MelodySequence Generate()
        {
            Random random = new Random();
            return Generate(sequence, random.Next());
        }

        public MelodySequence Next()
        {
            return Generate();
        }

        public bool HasNext
        {
            get { return true; }
        }
    }

    
}
