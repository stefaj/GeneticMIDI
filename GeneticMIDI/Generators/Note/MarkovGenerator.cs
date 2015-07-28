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
    class MarkovGenerator : INoteGenerator
    {
        Dictionary<Note, int> note_map;
        HiddenMarkovModel hmm;


        int max_length = 0;
        int[][] pitches;

        public MarkovGenerator(MelodySequence[] seqs)
        {

            int count = seqs.Length;
            note_map = new Dictionary<Note, int>();
            
            int pitch = 0;
            int j = 0;
            pitches = new int[seqs.Length][];
            foreach (MelodySequence m in seqs)
            {
                Note[] song = m.ToArray();
                pitches[j] = new int[m.Length];

                for (int i = 0; i < song.Length; i++)
                {
                    if (note_map.ContainsKey(song[i]))
                    {

                    }
                    else
                    {
                        note_map[song[i]] = pitch++;
                    }
                    pitches[j][i] = note_map[song[i]];
                }
                j++;
                if (m.Length > max_length)
                    max_length = m.Length;
            }


            hmm = new HiddenMarkovModel(3, pitch+1);
            var teacher = new BaumWelchLearning(hmm);
            teacher.Iterations = 10000;
            teacher.Run(pitches);
            
            
            Console.WriteLine("Done training");

            
        }
        
        public MelodySequence Generate()
        {
            Dictionary<int, Note> reverse_note_map = new Dictionary<int, Note>();

            foreach (var k in note_map.Keys)
                reverse_note_map[note_map[k]] = k;

            var notes1 = hmm.Generate(max_length);
            Note[] notes3 = new Note[notes1.Length];
            for (int i = 0; i < notes1.Length; i++)
            {
                notes3[i] = reverse_note_map[notes1[i]];
                //notes3[i] = reverse_note_map[pitches[0][i]];
            }
            return new MelodySequence(notes3);

        }


        public MelodySequence Next()
        {
            throw new NotImplementedException();
        }

        public bool HasNext
        {
            get { throw new NotImplementedException(); }
        }
    }
}
