using AForge.Genetic;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public enum NoteNames { C, Cs, D, Ds, E, F, Fs, G, GS, A, As, B };

    public enum Durations { tn=1, sn=2, en=4, qn=8, hn=16, wn=32, bn=64};
    public class Note 
    {
        public int Pitch { get; set; }
        public int Velocity { get; set; }
        public int Duration { get; set; } // wn = 16; bn 32; hn 8;

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
            this.Velocity = volume;
        }

        public Note(NoteNames chromatic_tone, int octave, Durations duration, int volume=127)
        {
            this.Pitch = (int)(chromatic_tone + 12 * octave);
            this.Duration = (int)duration;
            this.Velocity = volume;
        }

        public override string ToString()
        {
            return "(" + NoteNames[this.NotePitch] + this.Octave + "-" + (int)(this.Duration / 100.0f * 8.0f) + ")";
        }

        public static float ToRealDuration(int note_duration, int bpm=120)
        {
            return note_duration * 60.0f * 4.0f / 32.0f / bpm;
        }

        public static int ToNoteLength(int note_length, int delta_ticks_qn, double tempo)
        {
            return (int)((double)note_length / (double)delta_ticks_qn * (int)Durations.qn * (60 / tempo));
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

        public static Note[] LoadFromFile(string filename, int track=0)
        {
            List<Note> notes = new List<Note>();

            NAudio.Midi.MidiFile f = new MidiFile(filename);
            //f.Events.MidiFileType = 0;
            TempoEvent lastTempo = new TempoEvent(f.DeltaTicksPerQuarterNote, 0);
            lastTempo.Tempo = 60;
            foreach (var e in f.Events[track])
            {
                if (e as TempoEvent != null)
                    lastTempo = (TempoEvent)e;

                NoteOnEvent on = e as NoteOnEvent;
                if (on != null && on.OffEvent != null)
                {

                    double duration = ToNoteLength(on.NoteLength, f.DeltaTicksPerQuarterNote, lastTempo.Tempo);
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
            return p.Duration == this.Duration && p.Velocity == this.Velocity && p.Pitch == this.Pitch;
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
            return 3571 * this.Duration + 2903 * this.Pitch + 2129 * this.Velocity;
        }
    }

    
}
