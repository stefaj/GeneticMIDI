using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Harmony
{

    public class SimpleChord
    {
        #region CHORDS
        int[][] c = 
        {
new int[]{(int)NoteNames.C, (int)NoteNames.E, (int)NoteNames.G},
new int[]{(int)NoteNames.C, (int)NoteNames.E, (int)NoteNames.G},
new int[]{(int)NoteNames.C, (int)NoteNames.E, (int)NoteNames.G, (int)NoteNames.B},
new int[]{(int)NoteNames.C, (int)NoteNames.E, (int)NoteNames.G, (int)NoteNames.B},
new int[]{(int)NoteNames.C, (int)NoteNames.E, (int)NoteNames.G, (int)NoteNames.B},
new int[]{(int)NoteNames.C, (int)NoteNames.F, (int)NoteNames.G},
new int[]{(int)NoteNames.C, (int)NoteNames.F, (int)NoteNames.G, (int)NoteNames.B},
new int[]{(int)NoteNames.C, (int)NoteNames.E, (int)NoteNames.G, (int)NoteNames.A},
new int[]{(int)NoteNames.C, (int)NoteNames.D, (int)NoteNames.E, (int)NoteNames.G},
};
        int[][] d = 
        {
new int[]{(int)NoteNames.D, (int)NoteNames.Fs, (int)NoteNames.A},
new int[]{(int)NoteNames.D, (int)NoteNames.F, (int)NoteNames.A},
new int[]{(int)NoteNames.D, (int)NoteNames.Fs, (int)NoteNames.A, (int)NoteNames.C},
new int[]{(int)NoteNames.D, (int)NoteNames.Fs, (int)NoteNames.A, (int)NoteNames.Cs},
new int[]{(int)NoteNames.D, (int)NoteNames.F, (int)NoteNames.A, (int)NoteNames.C},
new int[]{(int)NoteNames.D, (int)NoteNames.G, (int)NoteNames.A},
new int[]{(int)NoteNames.D, (int)NoteNames.G, (int)NoteNames.A, (int)NoteNames.C},
new int[]{(int)NoteNames.D, (int)NoteNames.Fs, (int)NoteNames.A, (int)NoteNames.B},
new int[]{(int)NoteNames.D, (int)NoteNames.E, (int)NoteNames.Fs, (int)NoteNames.A},
};

        int[][] e = 
        {
new int[]{(int)NoteNames.E, (int)NoteNames.GS, (int)NoteNames.B},
new int[]{(int)NoteNames.E, (int)NoteNames.G, (int)NoteNames.B},
new int[]{(int)NoteNames.E, (int)NoteNames.GS, (int)NoteNames.B, (int)NoteNames.D},
new int[]{(int)NoteNames.E, (int)NoteNames.GS, (int)NoteNames.B, (int)NoteNames.Ds},
new int[]{(int)NoteNames.E, (int)NoteNames.G, (int)NoteNames.B, (int)NoteNames.D},
new int[]{(int)NoteNames.E, (int)NoteNames.A, (int)NoteNames.B},
new int[]{(int)NoteNames.E, (int)NoteNames.A, (int)NoteNames.B, (int)NoteNames.D},
new int[]{(int)NoteNames.E, (int)NoteNames.GS, (int)NoteNames.B, (int)NoteNames.Cs},
new int[]{(int)NoteNames.E, (int)NoteNames.Fs, (int)NoteNames.GS, (int)NoteNames.B},
};

        int[][] f = 
        {
new int[]{(int)NoteNames.Fs, (int)NoteNames.As, (int)NoteNames.Cs},
new int[]{(int)NoteNames.Fs, (int)NoteNames.A, (int)NoteNames.Cs},
new int[]{(int)NoteNames.Fs, (int)NoteNames.As, (int)NoteNames.Cs, (int)NoteNames.E},
new int[]{(int)NoteNames.Fs, (int)NoteNames.A, (int)NoteNames.Cs, (int)NoteNames.E},
new int[]{(int)NoteNames.Fs, (int)NoteNames.B, (int)NoteNames.Cs},
new int[]{(int)NoteNames.Fs, (int)NoteNames.B, (int)NoteNames.Cs, (int)NoteNames.E},
new int[]{(int)NoteNames.Fs, (int)NoteNames.As, (int)NoteNames.Cs, (int)NoteNames.Ds},
new int[]{(int)NoteNames.Fs, (int)NoteNames.GS, (int)NoteNames.As, (int)NoteNames.Cs},
};

        int[][] g = 
        {
new int[]{(int)NoteNames.G, (int)NoteNames.B, (int)NoteNames.D},
new int[]{(int)NoteNames.G, (int)NoteNames.B, (int)NoteNames.D, (int)NoteNames.F},
new int[]{(int)NoteNames.G, (int)NoteNames.B, (int)NoteNames.D, (int)NoteNames.Fs},
new int[]{(int)NoteNames.G, (int)NoteNames.C, (int)NoteNames.D},
new int[]{(int)NoteNames.G, (int)NoteNames.C, (int)NoteNames.D, (int)NoteNames.F},
new int[]{(int)NoteNames.G, (int)NoteNames.B, (int)NoteNames.D, (int)NoteNames.E},
new int[]{(int)NoteNames.G, (int)NoteNames.A, (int)NoteNames.B, (int)NoteNames.D},
};

        int[][] a = 
        {
new int[]{(int)NoteNames.A, (int)NoteNames.Cs, (int)NoteNames.E},
new int[]{(int)NoteNames.A, (int)NoteNames.C, (int)NoteNames.E},
new int[]{(int)NoteNames.A, (int)NoteNames.Cs, (int)NoteNames.E, (int)NoteNames.G},
new int[]{(int)NoteNames.A, (int)NoteNames.Cs, (int)NoteNames.E, (int)NoteNames.GS},
new int[]{(int)NoteNames.A, (int)NoteNames.C, (int)NoteNames.E, (int)NoteNames.G},
new int[]{(int)NoteNames.A, (int)NoteNames.D, (int)NoteNames.E},
new int[]{(int)NoteNames.A, (int)NoteNames.D, (int)NoteNames.E, (int)NoteNames.G},
new int[]{(int)NoteNames.A, (int)NoteNames.Cs, (int)NoteNames.E, (int)NoteNames.Fs},
new int[]{(int)NoteNames.A, (int)NoteNames.B, (int)NoteNames.Cs, (int)NoteNames.E},
};

        int[][] b = 
        {
new int[]{(int)NoteNames.B, (int)NoteNames.Ds, (int)NoteNames.Fs},
new int[]{(int)NoteNames.B, (int)NoteNames.D, (int)NoteNames.Fs},
new int[]{(int)NoteNames.B, (int)NoteNames.Ds, (int)NoteNames.Fs, (int)NoteNames.A},
new int[]{(int)NoteNames.B, (int)NoteNames.Ds, (int)NoteNames.Fs, (int)NoteNames.As},
new int[]{(int)NoteNames.B, (int)NoteNames.D, (int)NoteNames.Fs, (int)NoteNames.A},
new int[]{(int)NoteNames.B, (int)NoteNames.E, (int)NoteNames.Fs},
new int[]{(int)NoteNames.B, (int)NoteNames.E, (int)NoteNames.Fs, (int)NoteNames.A},
new int[]{(int)NoteNames.B, (int)NoteNames.Ds, (int)NoteNames.Fs, (int)NoteNames.GS},
new int[]{(int)NoteNames.B, (int)NoteNames.Cs, (int)NoteNames.Ds, (int)NoteNames.Fs},
};
        #endregion

        Random r;
        public SimpleChord()
        {
            r = new Random();
        }

        public HarmonySequence GetHarmonySequence (MelodySequence seq)
        {
            HarmonySequence sequence = new HarmonySequence();
            foreach(Note n in seq.ToArray())
            {
                if (n.Pitch > 0 && n.Velocity > 0 && n.Duration > (int)Durations.en)
                    sequence.AddChord(GetChord(n));
                else
                    seq.AddPause(n.Duration);
            }
            return sequence;

            
        }
        private Chord GetChord(Note n)
        {
            int[] pitches;
            if(n.NotePitch == (int)NoteNames.A || n.NotePitch == (int)NoteNames.As )
            {
                int i = r.Next(0, a.Length);
                pitches = a[i];
            }
            else if(n.NotePitch == (int)NoteNames.B)
            {
                int i = r.Next(0, b.Length);
                pitches = b[i];
            } 
            else if(n.NotePitch == (int)NoteNames.C || n.NotePitch == (int)NoteNames.Cs )
            {
                int i = r.Next(0, c.Length);
                pitches = c[i];
            } 
            else if(n.NotePitch == (int)NoteNames.D || n.NotePitch == (int)NoteNames.Ds )
            {
                int i = r.Next(0, d.Length);
                pitches = d[i];
            } 
            else if(n.NotePitch == (int)NoteNames.E)
            {
                int i = r.Next(0, e.Length);
                pitches = e[i];
            } 
            else if(n.NotePitch == (int)NoteNames.F || n.NotePitch == (int)NoteNames.Fs )
            {
                int i = r.Next(0, f.Length);
                pitches = f[i];
            } 
            else if(n.NotePitch == (int)NoteNames.G || n.NotePitch == (int)NoteNames.GS )
            {
                int i = r.Next(0, g.Length);
                pitches = g[i];
            } 
            else
            {
                int i = r.Next(0, a.Length);
                pitches = a[i];
            }

            for (int i = 0; i < pitches.Length; i++)
                pitches[i] = 12 * n.Octave + pitches[i];
            Chord ch = new Chord(pitches, n.Duration, n.Velocity/2);
            return ch;
        }
    }
}
