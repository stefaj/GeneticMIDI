using Accord.MachineLearning.Bayes;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators
{
    class MarkovGenerator : IGenerator
    {
        HiddenMarkovModel pitch_hmm;
        HiddenMarkovModel duration_hmm;
        NaiveBayes duration_bayes;
        public MarkovGenerator(string path)
        {
            
            string[] files = System.IO.Directory.GetFiles(path);
            int count = files.Count();

            List<int[]> pitches = new List<int[]>();
            List<int[]> durations = new List<int[]>();

            List<int[]> bayesInputs = new List<int[]>();
            List<int> bayesOutputs = new List<int>();

            if(File.Exists(path + "\\" + "pitch_model.dat"))
            {
                Console.WriteLine("Loading from file");
                pitch_hmm = HiddenMarkovModel.Load(path + "\\" + "pitch_model.dat");
                duration_hmm = HiddenMarkovModel.Load(path + "\\" + "duration_model.dat");
                duration_bayes = NaiveBayes.Load(path + "\\" + "duration_bayes.dat");
                return;
            }

            int max_notes = 0;
            foreach (string file in files)
            {
                if (System.IO.Path.GetExtension(file).ToLower() != ".mid")
                    continue;
                Note[] song;
                try
                {
                    song = Note.LoadFromFileSampled(file);
                }
                catch
                {
                    continue;
                }
                int[] _pitches = new int[song.Length];
                int[] _durations = new int[20];

                for(int i = 0; i < song.Length; i++)
                {
                    _pitches[i] = song[i].Pitch;
                    if(i<20)
                        _durations[i] = song[i].Duration;

                    if(i>3)
                    {
                        bayesInputs.Add(new int[]{_pitches[i-2],song[i-2].Duration,_pitches[i-1],song[i-1].Duration,_pitches[i]});
                        bayesOutputs.Add(song[i].Duration);
                    }
                }
                pitches.Add(_pitches);
                durations.Add(_durations);
                if (song.Length > max_notes)
                    max_notes = song.Length;
            }
            Console.WriteLine("Training Pitches");
            pitch_hmm = new HiddenMarkovModel(max_notes, 128);
            var teacher = new BaumWelchLearning(pitch_hmm) { Tolerance = 0.0001, Iterations = 0 };
            var __pitches = pitches.ToArray();
            teacher.Run(__pitches);
            pitch_hmm.Save(path + "\\" + "pitch_model.dat");

            Console.WriteLine("Training Durations");
            duration_hmm = new HiddenMarkovModel(20, 32);
            teacher = new BaumWelchLearning(duration_hmm) { Tolerance = 0.0001, Iterations = 0 };
            var __durations = durations.ToArray();
            teacher.Run(__durations);
            duration_hmm.Save(path + "\\" + "duration_model.dat");

            Console.WriteLine("Training Bayes");
            duration_bayes = new NaiveBayes(32, new int[] { 128, 32, 128, 32, 128 });
            duration_bayes.Estimate(bayesInputs.ToArray(), bayesOutputs.ToArray());
            duration_bayes.Save(path + "\\" + "duration_bayes.dat");


            Console.WriteLine("Done training");
            

        }
        public IEnumerable<Note> Generate()
        {
            var notes1 = pitch_hmm.Generate(100);
            var durs1 = duration_hmm.Generate(20);
            var durs = new int[100];
            for (int i = 0; i < durs1.Length; i++)
                durs[i] = durs1[i];
            Note[] notes3 = new Note[notes1.Length];
            for (int i = 0; i < notes1.Length; i++)
            {

                int dur;
                if(i>10)
                {
                    dur = duration_bayes.Compute(new int[] { notes1[i - 2], durs[i - 2], notes1[i - 1], durs[i - 1], notes1[i] });
                }
                else
                     dur = durs[i];
                if (dur < 1)
                    dur = durs1[i % 20];
                durs[i] = dur;
                notes3[i] = new Note(notes1[i], dur);
            }
            return notes3;

        }
    }
}
