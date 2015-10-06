using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Fractal
{
    public class ScaleType
    {
        public override string ToString()
        {
            return ScaleName;
        }

        public int[] GetScaleDegrees()
        {
            return SemitoneSkips;
        }

        private int NumScaleDegrees
        {
            get
            {
                return Enum.GetNames(typeof(NoteNamesWORest)).Length;
            }
        }

        public string ScaleName { get; private set; }
        public int[] SemitoneSkips { get; private set; }
        public ScaleType(string name, params int[] semitonesBetweenScaleDegrees)
        {
            ScaleName = name;

            SemitoneSkips = new int[semitonesBetweenScaleDegrees.Length];
            int scaleEndCheck = 0;
            int j = 0;
            foreach (int i in semitonesBetweenScaleDegrees)
            {
                SemitoneSkips[j++] = i;
                scaleEndCheck += i;
            }
            if ((scaleEndCheck % NumScaleDegrees) != 0)
            {
                // The start and end note of the scale are not the same!
                // Consider this a "compiler error"
                throw new Exception("ScaleType internal constructor error: " +
                        "Scale degrees must be a multiple of " +
                        NumScaleDegrees +
                        "; saw " + scaleEndCheck);
            }
        }

    }

    public static class Scales
    {
              public static ScaleType[] ScaleTypes = new ScaleType[] {
        new ScaleType("Chromatic", 1,1,1,1,1,1,1,1,1,1,1,1),
        new ScaleType("Major", 2,2,1,2,2,2,1), 
        new ScaleType("Harmonic Minor", 2,1,2,2,1,3,1),
        new ScaleType("Natural Minor",2,1,2,2,1,2,2),
        new ScaleType("Pentatonic", 2,2,3,2,3),
        new ScaleType("Hexatonic Blues", 3,2,1,1,3,2),
        new ScaleType("Heptatonic Blues", 2,1,2,1,3,1,2),
        new ScaleType("Nine Note Blues", 2,1,1,1,2,2,1,1,1),
        new ScaleType("Mode of Limited Transposition #1", 2,2,2,2,2,2),
        /*new ScaleType("Mode of Limited Transposition #2", 1,2,1,2,1,2,1,2),
        new ScaleType("Mode of Limited Transposition #3", 2,1,1,2,1,1,2,1,1),
        new ScaleType("Mode of Limited Transposition #4", 1,1,3,1,1,1,3,1),
        new ScaleType("Mode of Limited Transposition #5", 1,4,1,1,4,1),
        new ScaleType("Mode of Limited Transposition #6", 2,2,1,1,2,2,1,1),
        new ScaleType("Mode of Limited Transposition #7", 1,1,1,2,1,1,1,1,2,1)*/
    };
    }
}