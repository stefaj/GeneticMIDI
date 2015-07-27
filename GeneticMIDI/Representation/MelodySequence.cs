using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class MelodySequence : ISequence, ICloneable
    {
        List<Note> sequence;

        public int Duration { get; private set; }

        public float RealDuration
        {
            get
            {
                return Note.ToRealDuration(Duration);
            }
        }

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

        public MelodySequence(IEnumerable<Note> notes)
        {
            sequence = new List<Note>();
            foreach (Note n in notes)
                AddNote(n);
        }

        public void AddMelodySequence(MelodySequence seq)
        {
            foreach (Note n in seq.ToArray())
                AddNote(n);
        }

        public void TrimLeadingRests()
        {
            Note[] seq = new Note[sequence.Count];
            sequence.CopyTo(seq);
            foreach (Note n in seq)
            {
                if (n.Pitch < 0 || n.Velocity == 0)
                    sequence.Remove(n);
                else
                    break;
            }
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
            foreach (Note n in notes)
            {
                AddNote(n);
            }
        }

        public void AddPause(int d)
        {
            sequence.Add(new Note(-1, d, 0));
            Duration += d;
        }

        public Note[] ToArray()
        {
            return sequence.ToArray();
        }

        public void Transpose(int ht)
        {
            foreach (Note n in sequence)
                n.Pitch+=ht;
        }

        public string GetNoteStr()
        {
            string noteStr = "";
            foreach (Note n in sequence)
            {
                noteStr += n.ToString();
            }
            return noteStr;
        }

        public static string GetNoteStr(IEnumerable<Note> notes)
        {
            string noteStr = "";
            foreach (Note n in notes)
            {
                noteStr += n.ToString();
            }
            return noteStr;
        }

        public PlaybackInfo GeneratePlaybackInfo(byte channel, int time = 0)
        {
            PlaybackInfo info = new PlaybackInfo();
            foreach (Note n in sequence)
            {
                if (n.Pitch < 0 || n.Velocity == 0)
                {
                    time += (int)(1000 * Note.ToRealDuration(n.Duration));
                    continue;
                }
                PlaybackMessage m = new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Start, channel, (byte)n.Velocity, (byte)n.Pitch, n.Duration);
                info.Add(time, m);
                time += (int)(1000 * Note.ToRealDuration(n.Duration));
                m = new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Stop, channel, (byte)n.Velocity, (byte)n.Pitch);
                info.Add(time, m);
                time += 1;
            }
            return info;
        }


        public object Clone()
        {
            MelodySequence seq = new MelodySequence();
            foreach (Note n in sequence)
                seq.AddNote(n.Clone() as Note);
            return seq;
        }
    }
}
