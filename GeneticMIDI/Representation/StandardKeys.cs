using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public static class StandardKeys
    {
        public const int A = (int)NoteNames.A;
        public const int B = (int)NoteNames.B;
        public const int C = (int)NoteNames.C;
        public const int D = (int)NoteNames.D;
        public const int E = (int)NoteNames.E;
        public const int F = (int)NoteNames.F;
        public const int G = (int)NoteNames.G;

        public const int As = (int)NoteNames.As;
        public const int Cs = (int)NoteNames.Cs;
        public const int Ds = (int)NoteNames.Ds;
        public const int Fs = (int)NoteNames.Fs;
        public const int Gs = (int)NoteNames.GS;

        public const int Ab = Gs;
        public const int Bb = As;
        public const int Db = Cs;
        public const int Eb = Ds;
        public const int Gb = Fs;

        public static int[] C_MAJOR = { C, D, E, F, G, A, B, C };
        public static int[] D_MAJOR = { D, E, Fs, G, A, B, Cs, D };
        public static int[] E_MAJOR = { E, Fs, Gs, A, B, Cs, Ds, E };
        public static int[] F_MAJOR = { F, G, A, Bb, C, D, E, F };
        public static int[] G_MAJOR = { G, A, B, C, D, E, Fs, G };
        public static int[] A_MAJOR = { A, B, Cs, D, E, Fs, Gs, A };
        public static int[] B_MAJOR = { B, Cs, Ds, E, Fs, Gs, As, B };
        public static int[] CS_MAJOR = { Db, Eb, F, Gb, Ab, Bb, C, Db }; //Db
        public static int[] DS_MAJOR = { Eb, F, G, Ab, Bb, C, D, Eb }; //Eb
        public static int[] FS_MAJOR = { Fs, Gs, As, B, Cs, Ds, F, Fs }; //Gb
        public static int[] GS_MAJOR = { Ab, Bb, C, Db, Eb, F, G, Ab }; //Ab
        public static int[] AS_MAJOR = { Bb, C, D, Eb, F, G, A, Bb }; //Bb

        public static int[][] MAJOR_KEYS = { C_MAJOR, D_MAJOR, E_MAJOR, F_MAJOR, G_MAJOR, A_MAJOR, B_MAJOR, CS_MAJOR, DS_MAJOR, FS_MAJOR, GS_MAJOR, AS_MAJOR };
    }
}
