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
        }

        public void AddNote(NoteNames name, int octave, Durations d)
        {
            AddNote(new Note(name, octave, d));
        }

        public void AddNote(Note n)
        {
            sequence.Add(n);
            Duration += n.Duration;
        }

        public void AddNotes(IEnumerable<Note> notes)
        {
            foreach(Note n in notes)
            {
                AddNote(n);
            }
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

        public PlaybackInfo GeneratePlaybackInfo(byte channel, int time = 0)
        {
            PlaybackInfo info = new PlaybackInfo();
            foreach (Note n in sequence)
            {
                if (n.Pitch < 0 || n.Velocity == 0)
                    continue;
                PlaybackMessage m = new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Start, channel, (byte)n.Velocity, (byte)n.Pitch);
                info.Add(time, m);
                time += (int)(1000 * Note.ToRealDuration(n.Duration));
                m = new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Stop, channel, (byte)n.Velocity, (byte)n.Pitch);
                info.Add(time, m);
                time += 1;
            }
            return info;
        }

    }
}
