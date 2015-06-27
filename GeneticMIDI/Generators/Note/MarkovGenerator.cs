using Accord.MachineLearning.Bayes;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using GeneticMIDI.Representation;
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
        Dictionary<int, int> pitch_map;
        Dictionary<int, int> duration_map;
        public MarkovGenerator(string path)
        {
            
            string[] files = System.IO.Directory.GetFiles(path);
            int count = files.Count();

            List<int[]> pitches = new List<int[]>();
            List<int[]> durations = new List<int[]>();

            List<int[]> bayesInputs = new List<int[]>();
            List<int> bayesOutputs = new List<int>();

            if (File.Exists(path + "\\" + "duration_bayes.dat"))
            {
                Console.WriteLine("Loading from file");
                pitch_hmm = HiddenMarkovModel.Load(path + "\\" + "pitch_model.dat");
                duration_hmm = HiddenMarkovModel.Load(path + "\\" + "duration_model.dat");
                duration_bayes = NaiveBayes.Load(path + "\\" + "duration_bayes.dat");
                return;
            }

            int max_notes = 0;
            Dictionary<int, int> pitch_map = new Dictionary<int, int>();
            Dictionary<int, int> duration_map = new Dictionary<int, int>();
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
                    int dur = song[i].Duration;
                    if (dur > 31)
                        dur = 31;
                    if(i<20)
                        _durations[i] = dur;

                    if(i>3)
                    {
                        int dur2 = song[i - 2].Duration;
                        int dur1 = song[i - 1].Duration;
                        if (dur2 > 31)
                            dur2 = 31;
                        if (dur1 > 31)
                            dur1 = 31;
                        bayesInputs.Add(new int[]{_pitches[i-2],dur2,_pitches[i-1],dur1,_pitches[i]});
                        bayesOutputs.Add(dur);
                    }
                }
                pitches.Add(_pitches);
                durations.Add(_durations);
                if (song.Length > max_notes)
                    max_notes = song.Length;
            }
            Console.WriteLine("Training Pitches");
            pitch_hmm = new HiddenMarkovModel(max_notes, 128);
            var teacher = new BaumWelchLearning(pitch_hmm) { Tolerance = 0.001, Iterations = 0 };
            var __pitches = pitches.ToArray();
            teacher.Run(__pitches);
            pitch_hmm.Save(path + "\\" + "pitch_model.dat");

            Console.WriteLine("Training Durations");
            duration_hmm = new HiddenMarkovModel(20, 32);
            teacher = new BaumWelchLearning(duration_hmm) { Tolerance = 0.001, Iterations = 0 };
            var __durations = durations.ToArray();
            teacher.Run(__durations);
            duration_hmm.Save(path + "\\" + "duration_model.dat");

            Console.WriteLine("Training Bayes");
            duration_bayes = new NaiveBayes(32, new int[] { 128, 32, 128, 32, 128 });
            duration_bayes.Estimate(bayesInputs.ToArray(), bayesOutputs.ToArray());
            duration_bayes.Save(path + "\\" + "duration_bayes.dat");


            Console.WriteLine("Done training");
            

        }


        public MarkovGenerator(MelodySequence[] seqs)
        {

            int count = seqs.Length;

            List<int[]> pitches = new List<int[]>();
            List<int[]> durations = new List<int[]>();

            List<int[]> bayesInputs = new List<int[]>();
            List<int> bayesOutputs = new List<int>();
            pitch_map = new Dictionary<int, int>();
            duration_map = new Dictionary<int, int>();

            int max_notes = 0;
          
            int pitch = 0;
            int dur_ = 0;
            foreach (MelodySequence m in seqs)
            {
                Note[] song = m.ToArray();

                int[] _pitches = new int[song.Length];
                int[] _durations = new int[20];

                for (int i = 0; i < song.Length; i++)
                {
                    if(pitch_map.ContainsKey(song[i].Pitch))
                        _pitches[i] = pitch_map[song[i].Pitch];
                    else
                    {
                        pitch_map[song[i].Pitch] = pitch++;
                        _pitches[i] = pitch_map[song[i].Pitch];
                    }

                    int dur = song[i].Duration;
 
                    if (duration_map.ContainsKey(dur))
                    {
                        if (i < 20)
                            _durations[i] =  duration_map[dur];
                    }
                    else
                    {
                        duration_map[dur] = dur_++;
                    }
                    if (i > 3)
                    {
                        int dur2 = duration_map[song[i - 2].Duration];
                        int dur1 = duration_map[song[i - 1].Duration];

                        bayesInputs.Add(new int[] { _pitches[i - 2], dur2, _pitches[i - 1], dur1, _pitches[i] });
                        bayesOutputs.Add(dur);
                    }
                }
                pitches.Add(_pitches);
                durations.Add(_durations);
                if (song.Length > max_notes)
                    max_notes = song.Length;
            }
            int max_pitches = pitch_map.Keys.Count;
            int max_durs = duration_map.Keys.Count;
            Console.WriteLine("Training Pitches");
            pitch_hmm = new HiddenMarkovModel(max_pitches, max_pitches);
            var teacher = new BaumWelchLearning(pitch_hmm) { Tolerance = 0.001, Iterations = 0 };
            var __pitches = pitches.ToArray();
            teacher.Run(__pitches);

            Console.WriteLine("Training Durations");
            duration_hmm = new HiddenMarkovModel(max_durs, max_durs);
            teacher = new BaumWelchLearning(duration_hmm) { Tolerance = 0.001, Iterations = 0 };
            var __durations = durations.ToArray();
            teacher.Run(__durations);

            Console.WriteLine("Training Bayes");
            duration_bayes = new NaiveBayes(max_durs, new int[] { max_pitches, max_durs, max_pitches, max_durs, max_pitches });
            duration_bayes.Estimate(bayesInputs.ToArray(), bayesOutputs.ToArray());


            Console.WriteLine("Done training");


        }
        
        public IEnumerable<Note> Generate()
        {
            Dictionary<int, int> reverse_pitch_map = new Dictionary<int, int>();
            Dictionary<int, int> reverse_duration_map = new Dictionary<int, int>();
            foreach (int k in pitch_map.Keys)
                reverse_pitch_map[pitch_map[k]] = k;
            foreach (int k in duration_map.Keys)
                reverse_duration_map[duration_map[k]] = k;

            var notes1 = pitch_hmm.Generate(40);
            var durs1 = duration_hmm.Generate(20);
            var durs = new int[100];
            for (int i = 0; i < durs1.Length; i++)
                durs[i] = durs1[i];
            Note[] notes3 = new Note[notes1.Length];
            for (int i = 0; i < notes1.Length; i++)
            {

                int dur;
                if(i>2)
                {
                    dur = duration_bayes.Compute(new int[] { notes1[i - 2], durs[i - 2], notes1[i - 1], durs[i - 1], notes1[i] });
                }
                else
                     dur = durs[i];
                if (dur < 1)
                    dur = durs1[i % 20];
                durs[i] = dur;

                int duration = reverse_duration_map[dur];
                if (duration < (int)Durations.sn)
                    duration *= (int)Durations.sn;
                if (duration >= (int)Durations.bn)
                    duration /= (int)Durations.wn;
                //duration = (int)Durations.qn;
                notes3[i] = new Note(reverse_pitch_map[notes1[i]], duration);
            }
            return notes3;

        }
    }
}
