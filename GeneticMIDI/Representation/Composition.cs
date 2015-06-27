using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class Composition
    {
        public List<Track> Tracks { get; private set; }

        public Composition()
        {
            this.Tracks = new List<Track>();
        }

        public void Add(Track track)
        {
            this.Tracks.Add(track);
        }

        public PlaybackInfo GeneratePlaybackInfo()
        {
            PlaybackInfo info = new PlaybackInfo();
            foreach (Track t in Tracks)
                info += t.GeneratePlaybackInfo(0);
            return info;
        }

        public Track GetLongestTrack()
        {
            int max_dur = 0;
            int id = 0;
            for (int i = 0; i < Tracks.Count; i++)
            {
                var t = Tracks[i];
                if (t.Duration > max_dur)
                {
                    max_dur = t.Duration;
                    id = i;
                }
            }
            return Tracks[id];
        }

        public void LoadFromMIDI(string filename)
        {
            Tracks.Clear();

            NAudio.Midi.MidiFile f = new MidiFile(filename);
            f.Events.MidiFileType = 0;


            byte max_channels = 0;
            foreach (var e in f.Events[0])
                if (e.Channel > max_channels)
                    max_channels = (byte)e.Channel;
            max_channels++;
            Track[] tracks = new Track[max_channels];
            MelodySequence[] seqs = new MelodySequence[max_channels];
            TempoEvent[] tempos = new TempoEvent[max_channels];
            for (byte i = 0; i < max_channels; i++ )
            {
                tracks[i] = new Track(PatchNames.Acoustic_Grand, i);
                seqs[i] = new MelodySequence();
                tempos[i] = new TempoEvent((int)(Note.ToRealDuration((int)Durations.qn, 60) * 1000), 0);
                tempos[i].Tempo = 60;
            }
            foreach (var e in f.Events[0])
            {
                if (e as TempoEvent != null)
                {
                    //tempos[e.Channel] = (TempoEvent)e;
                }
                if (e as PatchChangeEvent != null)
                {
                    var p = e as PatchChangeEvent;
                    tracks[p.Channel].Instrument = (PatchNames)p.Patch;
                }
                NoteOnEvent on = e as NoteOnEvent;
                if (on != null && on.OffEvent != null)
                {
                    int total_dur = Note.ToNoteLength((int)on.AbsoluteTime, f.DeltaTicksPerQuarterNote, tempos[on.Channel].Tempo);
                    if (total_dur > seqs[on.Channel].Duration)
                        seqs[on.Channel].AddPause(total_dur - seqs[on.Channel].Duration);
                    
                    int duration = Note.ToNoteLength(on.NoteLength, f.DeltaTicksPerQuarterNote, tempos[on.Channel].Tempo);
                    seqs[on.Channel].AddNote(new Note(on.NoteNumber, (int)duration));
                }
            }
            for(byte i = 0; i < max_channels; i++)
            {
                if(seqs[i].Length > 0)
                {
                    tracks[i].AddSequence(seqs[i]);
                    Tracks.Add(tracks[i]);
                }
            }
        }
    }
}
