using AForge.Genetic;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI
{
    public class Note 
    {
        public int Pitch { get; set; }
        public int Volume { get; set; }
        public int Duration { get; set; } // 1/8 th note

        public int Octave { get { return Pitch / 12; }
            set
            {
                int notepitch = NotePitch;
                int octave = value;
                Pitch = octave * 12 + notepitch;
            }
        }

        public int NotePitch
        {
            get
            {
                return Pitch % 12;
            }
            set
            {
                int octave = Octave;
                int notepitch = value;
                Pitch = octave * 12 + notepitch;
            }
        }
        public Note(int pitch, int duration, int volume = 127)
        {
            this.Pitch = pitch;
            this.Duration = duration;
            this.Volume = volume;
        }

        public override string ToString()
        {
            return "(" + NoteNames[this.NotePitch] + this.Octave + "-" + (int)(this.Duration / 100.0f * 8.0f) + ")";
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

        private static readonly string[] NoteNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static Note[] LoadFromFile(string filename)
        {
            List<Note> notes = new List<Note>();

            NAudio.Midi.MidiFile f = new MidiFile(filename);
            f.Events.MidiFileType = 0;
            TempoEvent lastTempo = new TempoEvent(f.DeltaTicksPerQuarterNote, 0);
            foreach (var e in f.Events[0])
            {
                if (e as TempoEvent != null)
                    lastTempo = (TempoEvent)e;

                NoteOnEvent on = e as NoteOnEvent;
                if (on != null && on.OffEvent != null)
                {
                    double duration = (on.NoteLength / (lastTempo.Tempo)) * 8;
                    notes.Add(new Note(on.NoteNumber, (int)duration));
                }
            }
            return notes.ToArray(); 
        }

        public override bool Equals(object obj)
        {
            Note p = obj as Note;
            if (p == null)
                return false;
            return p.Duration == this.Duration && p.Volume == this.Volume && p.Pitch == this.Pitch;
        }

        public static bool operator == (Note n1, Note n2)
        {
            return n1.Equals(n2);
        }
        public static bool operator != (Note n1, Note n2)
        {
            return !n1.Equals(n2);
        }

        public static Note[] LoadFromFileSampledSpaced(string filename)
        {
            List<Note> notes = new List<Note>();

            NAudio.Midi.MidiFile f = new MidiFile(filename);
            f.Events.MidiFileType = 0;
            TempoEvent lastTempo = new TempoEvent(f.DeltaTicksPerQuarterNote, 0);

            int interval = f.DeltaTicksPerQuarterNote;
            int i = 0;
            int start = 0;
            while(i < f.Events[0].Count)
            {
                var e = f.Events[0][i];

                if (e as TempoEvent != null)
                    lastTempo = (TempoEvent)e;
                NoteOnEvent on = e as NoteOnEvent;
                if (on != null && on.OffEvent != null)
                {
                    if (on.AbsoluteTime <= start + interval && on.AbsoluteTime >= start)
                    {
                        notes.Add(new Note(on.NoteNumber, (int)(on.NoteLength / lastTempo.Tempo) * 60));
                        start += interval;
                    }
                    else if (on.AbsoluteTime > start)
                    {
                        start += interval;
                        continue;
                    }
                }
                i++;
            }
            return notes.ToArray();
        }

        public static Note[] LoadFromFileSampled(string filename)
        {
            List<Note> notes = new List<Note>();

            NAudio.Midi.MidiFile f = new MidiFile(filename);
            f.Events.MidiFileType = 0;
            TempoEvent lastTempo = new TempoEvent(f.DeltaTicksPerQuarterNote, 0);

            int interval = f.DeltaTicksPerQuarterNote;
            int i = 0;
            int start = 0;
            while (i < f.Events[0].Count)
            {
                var e = f.Events[0][i];

                if (e as TempoEvent != null)
                    lastTempo = (TempoEvent)e;
                NoteOnEvent on = e as NoteOnEvent;
                if (on != null && on.OffEvent != null)
                {
                    if (on.AbsoluteTime <= start + interval && on.AbsoluteTime >= start)
                    {
                        notes.Add(new Note(on.NoteNumber, (int)((8*on.NoteLength) / lastTempo.Tempo)));
                        while (start + interval < on.AbsoluteTime + on.NoteLength)
                        {
                            start += interval;
                        }
                    }
                    else if (on.AbsoluteTime > start)
                    {
                        start += interval;
                        continue;
                    }
                }
                i++;
            }
            return notes.ToArray();
        }

        public override int GetHashCode()
        {
            return 3571 * this.Duration + 2903 * this.Pitch + 2129 * this.Volume;
        }
    }

    
}
