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

        public static Composition GetRandomComposition(string path)
        {
            return Composition.LoadFromMIDI(GetRandomMidiFile(path));
        }
    }
}
