using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    class StandardChord : Chord
    {
        const int CHORD_FLAVORS = 12;

        enum Flavors
        {
            MAJOR = 407, MAJOR7 = 410, MAJOR6 = 509, MAJOR76 = 206, MAJOR4 = 308, MAJOR74 = 608, MINOR = 307, MINOR7 = 310,
            MINOR6 = 508, MINOR76 = 205, MINOR4 = 409, MINOR74 = 709, DIM = 306, DIM6 = 609, DIM4 = 309, AUG = 408, OTHER = 999
        };

        public int LowPitch{get; private set;}
        public int MiddlePitch{get; private set;}
        public int HighPitch{get; private set;}

        public int Flavor{get; private set;}
        public int Code{get; private set;}

        public StandardChord(int pitch1, int pitch2, int pitch3, int duration)
            : base(new int[] { pitch1, pitch2, pitch3 }, duration)
        {
            var arr = new int[]{pitch1, pitch2, pitch3};
            Array.Sort(arr);

            LowPitch = arr[0];
            MiddlePitch = arr[1];
            HighPitch = arr[2];

            Flavor = (MiddlePitch - LowPitch) * 100 + HighPitch - LowPitch;
            Code = (LowPitch % 12 + 12) % 12 * 10000 + Flavor;
        }

        public bool IsMajor()
        {
            return Flavor == (int)Flavors.MAJOR || Flavor == (int)Flavors.MAJOR6 || Flavor == (int)Flavors.MAJOR4;
        }

        public bool IsMajor7()
        {
            return Flavor == (int)Flavors.MAJOR7 || Flavor == (int)Flavors.MAJOR76 || Flavor == (int)Flavors.MAJOR74;
        }

        public bool IsMinor()
        {
            return Flavor == (int)Flavors.MINOR || Flavor == (int)Flavors.MINOR6 || Flavor == (int)Flavors.MINOR4;
        }


        public bool IsMinor7()
        {
            return Flavor == (int)Flavors.MINOR7 || Flavor == (int)Flavors.MINOR76 || Flavor == (int)Flavors.MINOR74;
        }


        public bool IsDiminished()
        {
            return Flavor == (int)Flavors.DIM || Flavor == (int)Flavors.DIM6 || Flavor == (int)Flavors.DIM4;
        }


        public bool IsAugmented()
        {
            return Flavor == (int)Flavors.AUG;
        }

        public StandardChord normalize()
        {
            switch ((Flavors)Flavor)
            {
                case Flavors.MAJOR6:
                    return new StandardChord(MiddlePitch, MiddlePitch + 4, MiddlePitch + 7, this.Duration);
                case Flavors.MAJOR76:
                    return new StandardChord(MiddlePitch, MiddlePitch + 4, MiddlePitch + 10, this.Duration);
                case Flavors.MAJOR4:
                    return new StandardChord(HighPitch - 12, HighPitch - 8, HighPitch - 5, this.Duration);
                case Flavors.MAJOR74:
                    return new StandardChord(HighPitch - 12, HighPitch - 8, HighPitch - 2, this.Duration);
                case Flavors.MINOR6:
                    return new StandardChord(MiddlePitch, MiddlePitch + 3, MiddlePitch + 7, this.Duration);
                case Flavors.MINOR76:
                    return new StandardChord(MiddlePitch, MiddlePitch + 3, MiddlePitch + 10, this.Duration);
                case Flavors.MINOR4:
                    return new StandardChord(HighPitch - 12, HighPitch - 9, HighPitch - 5, this.Duration);
                case Flavors.MINOR74:
                    return new StandardChord(HighPitch - 12, HighPitch - 9, HighPitch - 2, this.Duration);
                case Flavors.DIM6:
                    return new StandardChord(MiddlePitch, MiddlePitch + 3, MiddlePitch + 6, this.Duration);
                case Flavors.DIM4:
                    return new StandardChord(HighPitch - 12, HighPitch - 9, HighPitch - 6, this.Duration);
                default:
                    return this;
            }
        }

        public StandardChord RotateUp()
        {
            return new StandardChord(this.LowPitch + 12, this.MiddlePitch, this.HighPitch, this.Duration);
        }

        public StandardChord RotateDown()
        {
            return new StandardChord(this.LowPitch, this.MiddlePitch, this.HighPitch-12, this.Duration);
        }

    }
}
