using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GeneticMIDI
{
    public static class Utils
    {
        public static Composition[] LoadCompositionsFromDirectory(string path)
        {
            var files = GetFiles(path);
            List<Composition> comps = new List<Composition>(files.Length);
            int i = 0;
            foreach(string s in files)
            {
                if (Path.GetExtension(s).ToLower() != ".mid")
                    continue;

                try
                {
                    Console.WriteLine("{0:00.00}%\t Loading {1}", (float)(i++) / files.Length * 100.0f, s);
                    Composition comp = Composition.LoadFromMIDI(s);
                    comps.Add(comp);
                }
                catch
                {

                }
            }

            return comps.ToArray();
        }

        public static Composition[] LoadCompositionsParallel(string path)
        {
            var files = GetFiles(path);
            List<Composition> comps = new List<Composition>(files.Length);
         
            Parallel.For(0, files.Length, i =>
                {
                    string s = files[i];
                    if (Path.GetExtension(s).ToLower() == ".mid")
                    {

                        try
                        {
                            Console.WriteLine("{0:00.00}%\t Loading {1}", (float)(i++) / files.Length * 100.0f, s);
                            Composition comp = Composition.LoadFromMIDI(s);
                            comps.Add(comp);
                        }
                        catch
                        {

                        }
                    }
                });
            return comps.ToArray();
        }

        public static MelodySequence[] LoadMelodySequencesFromDirectory(string path)
        {
            var comps = LoadCompositionsFromDirectory(path);
            MelodySequence[] mels = new MelodySequence[comps.Length];

            for (int i = 0; i < mels.Length; i++)
            {
                mels[i] = comps[i].GetLongestTrack().GetMainSequence() as MelodySequence;
            }

            return mels;
        }

        public static string[] GetFiles(string path, int level = 1)
        {
            if (level == 0)
                return Directory.GetFiles(path);
            else
            {
                List<string> files = new List<string>();
                files.AddRange(Directory.GetFiles(path));
                foreach(string dir in Directory.GetDirectories(path))
                {
                    files.AddRange(GetFiles(dir, level - 1));
                }
                return files.ToArray();
            }
        }

        public static string GetRandomMidiFile(string path)
        {
            var files = GetFiles(path);
            string f = "";

            System.Random rand = new Random();
            while(Path.GetExtension(f).ToLower() != ".mid")
            {
                f = files[rand.Next(0, files.Length)];
            }
            return f;
        }

        public static double[] GetDoublesFromNotes(Note[] notes, int max_length = 200)
        {
            int length = notes.Length > max_length ? max_length : notes.Length;
            double[] outarr = new double[max_length];
            int max = 128 * 128 + 64;

            int i = 0;
            for (i = 0; i < length; i++)
            {
                int o = notes[i].Pitch * 128 + notes[i].Duration;
                outarr[i] = (double)o / (double)max;
            }

            for (; i < max_length; i++)
                outarr[i] = 0;
            return outarr;
        }

        public static MelodySequence GetMelodyFromDoubles(double[] arr)
        {
            int max = 128 * 128 + 64;
            Note[] notes = new Note[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                int num = (int)(arr[i] * max);
                notes[i] = new Note(num / 128, num % 128);
            }
            return new MelodySequence(notes);
        }

        public static Dictionary<A, B> ReverseDictionary<A, B>(Dictionary<B, A> dictionary)
        {
            Dictionary<A, B> reverse_note_map = new Dictionary<A, B>();

            foreach (var k in dictionary.Keys)
                reverse_note_map[dictionary[k]] = k;
            return reverse_note_map;
        }


        public static Composition GetRandomComposition(string path)
        {
            return Composition.LoadFromMIDI(GetRandomMidiFile(path));
        }
    }
}
