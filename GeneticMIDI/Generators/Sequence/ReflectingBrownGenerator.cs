using GeneticMIDI.Fractal;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    public class ReflectingBrownNoteGenerator : BrownNoteGenerator, INoteGenerator
    {
        public int MaxNotes { get; set; }
        Random gen;

        public ReflectingBrownNoteGenerator(
                NoteRangeRestrictor nrr, Random randGen,
                int lowestPitchChange, int highestPitchChange,
                int lowestLengthStep, int highestLengthStep)
            : base(
                nrr, lowestPitchChange, highestPitchChange,
                lowestLengthStep, highestLengthStep
                )
        {
            this.MaxNotes = 200;
            gen = randGen;
        }


        public override int CalculateChange(int startVal, int maxNegChange, int maxPosChange, int maxIndexPlusOne)
        {
            int minVal = 0;
            int maxVal = maxIndexPlusOne - 1;
            int changeAmount = gen.Next(maxPosChange - maxNegChange + 1) + maxNegChange;
            int newVal = startVal + changeAmount;
            if ((newVal < minVal) || (newVal > maxVal))
            {
                newVal = startVal - changeAmount;
                if ((newVal < minVal) || (newVal > maxVal))
                {
                    newVal = startVal;
                }
            }

            return newVal;
        }

        public Representation.MelodySequence Generate()
        {
            List<Note> notes = new List<Note>();
            for (int i = 0; i < MaxNotes; i++)
            {
                var note = this.GetNextNote();
                notes.Add(note);
            }

            return new MelodySequence(notes);

        }

        public Representation.MelodySequence Next()
        {
            return Generate();
        }

        public bool HasNext
        {
            get { return true; }
        }
    }

}
