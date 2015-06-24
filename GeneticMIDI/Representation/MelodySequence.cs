using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class MelodySequence : ISequence
    {
        List<Note> sequence;

        public int Duration { get; private set; }

        public int Length
        {
            get
            {
                return sequence.Count;
            }
        }

        public MelodySequence()
        {
            sequence = new List<Note>();
            Duration = 0;
        }

        public void AddNote(int pitch, int d)
        {
            sequence.Add(new Note(pitch, d));
            Duration += d;
        }

        public void AddPause(int d)
        {
            sequence.Add(new Note(-1, d, 0));
            Duration += d;
        }

        public void Transpose(int ht)
        {
            foreach (Note n in sequence)
                n.Pitch++;
        }

        public SortedDictionary<int, IEnumerable<PlaybackMessage>> GeneratePlaybackData(byte channel, int time = 0)
        {
            SortedDictionary<int, IEnumerable<PlaybackMessage>> messages = new SortedDictionary<int, IEnumerable<PlaybackMessage>>();
            foreach (Note n in sequence)
            {
                if (n.Pitch < 0 || n.Velocity == 0)
                    continue;
                PlaybackMessage[] m = new PlaybackMessage[]{new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Start, channel, (byte)n.Velocity, (byte)n.Pitch)};

                messages.Add(time, m);
                time += (int)(1000 * Note.ToRealDuration(n.Duration));
                m = new PlaybackMessage[]{new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Stop, channel, (byte)n.Velocity, (byte)n.Pitch)};
                messages.Add(time, m);
                time += 1;
            }
            return messages;
        }

    }
}
