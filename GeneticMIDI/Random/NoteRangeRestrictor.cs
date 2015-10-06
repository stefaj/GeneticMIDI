using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Fractal
{
    public class NoteRangeRestrictor
    {
        private int[] pitchRange;
        private int[] lengthRange;

        /// <summary>
        /// Specify all restrictions that should be placed on the range of generated notes.
        /// </summary>
        /// <param name="lowPitch">The lowest pitch that can be generated.</param>
        /// <param name="highPitch">The highest pitch that can be generated.</param>
        /// <param name="shortLength">The shortest duration that can be generated.</param>
        /// <param name="longLength">The longest duration that can be generated.</param>
        /// <param name="scaleToUse">The scale to use to generate notes.</param>
        public NoteRangeRestrictor(int lowPitch, int highPitch,int shortLength, int longLength,ScaleType scaleToUse)
        {
            int nextPitch = lowPitch;
            int scaleIndex = -1;
            int[] scale = scaleToUse.GetScaleDegrees();
            var pitchRangeList = new List<int>();

            // Add notes based on the scale until we reach highPitch.
            while (nextPitch.CompareTo(highPitch) <= 0)
            {
                pitchRangeList.Add(nextPitch);
                scaleIndex = (scaleIndex + 1) % scale.Count();
                int semitoneIncrease = scale[scaleIndex];

                nextPitch = nextPitch + semitoneIncrease;
                if (nextPitch > 127)
                {
                    nextPitch--;
                    break;
                }

            }
            this.pitchRange = pitchRangeList.ToArray();
            this.lengthRange = Note.GetDurationRange(shortLength, longLength);
        }


        public int GetNumPitches() { return pitchRange.Count(); }
        public int GetNumDurations() { return lengthRange.Count(); }
        public int GetPitch(int index) { return pitchRange[index]; }
        public int GetDuration(int index) { return lengthRange[index]; }

    }
}
