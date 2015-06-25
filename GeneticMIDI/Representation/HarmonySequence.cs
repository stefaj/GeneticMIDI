using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class HarmonySequence : ISequence
    {
        List<Chord> sequence;

        public int Duration { get; private set; }

        public int Length
        {
            get
            {
                return sequence.Count;
            }
        }

        public HarmonySequence()
        {
            sequence = new List<Chord>();
            Duration = 0;
        }

        public void AddChord(Chord c)
        {
            sequence.Add(c);
            Duration += c.Duration;
        }

        public void AddPause(int d)
        {
            sequence.Add(new Chord(new int[]{},d,0));
            Duration += d;
        }

        public void Transpose(int ht)
        {
            foreach (Chord c in sequence)
                c.Transpose(ht);
        }

        public PlaybackInfo GeneratePlaybackInfo(byte channel, int time = 0)
        {
            PlaybackInfo info = new PlaybackInfo();
            foreach(Chord c in sequence)
            {
                if(c.Length == 0 || c.Velocity == 0)
                    continue;
                PlaybackMessage[] m = new PlaybackMessage[c.Length];
                for (int i = 0; i < c.Notes.Length; i++ )
                {
                    var note = c.Notes[i];
                    m[i] = new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Start, channel, (byte)c.Velocity, (byte)note.Pitch);
                }
                info.Add(time, m);
                time += (int)(1000 * Note.ToRealDuration(c.Duration));
                m = new PlaybackMessage[c.Length];
                for (int i = 0; i < c.Notes.Length; i++)
                {
                    var note = c.Notes[i];
                    m[i] = new PlaybackMessage(PlaybackMessage.PlaybackMessageType.Stop, channel, (byte)c.Velocity, (byte)note.Pitch);
                }
                info.Add(time, m);
                time += 1;
            }
            return info;
        }
    }
}
