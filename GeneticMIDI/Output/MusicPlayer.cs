using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class MusicPlayer
    {
        MidiOut midiOut;

        public MusicPlayer()
        {
            midiOut = new MidiOut(0);
        }

        
        public void Play(ISequence seq)
        {
            var msgs = seq.GeneratePlaybackInfo(1,0);
            Play(msgs);
        }

        public void Play(Composition comp)
        {
            var info = comp.GeneratePlaybackInfo();
            Play(info);
        }

        private void Play(PlaybackInfo info)
        {
            var keys = new int[info.Messages.Keys.Count];
            int i = 0;
            foreach(var k in info.Messages.Keys)
            {
                keys[i++] = k;
            }
            for(i = 0; i < keys.Length - 1; i++)
            {
                foreach(PlaybackMessage message in info.Messages[keys[i]])
                {
                    midiOut.Send(message.GenerateMidiMessage().RawData);
                }
                int sleep_dur = keys[i + 1] - keys[i];
                Thread.Sleep(sleep_dur);
            }

        }


        public void PlayChord(Chord c)
        {
            foreach (Note n in c.Notes)
            {
                midiOut.Send(MidiMessage.StartNote(n.Pitch, c.Velocity, 1).RawData);
            }

            Thread.Sleep((int)(Note.ToRealDuration(c.Duration) * 1000));
            foreach (Note n in c.Notes)
            {                
                midiOut.Send(MidiMessage.StopNote(n.Pitch, 0, 1).RawData);
            }
        }

        public void PlayChords(Chord[] chords)
        {
            foreach(Chord c in chords)
            {
                foreach (Note n in c.Notes)
                {
                    midiOut.Send(MidiMessage.StartNote(n.Pitch, c.Velocity, 1).RawData);
                }

                Thread.Sleep((int)(Note.ToRealDuration(c.Duration) * 1000));
                foreach (Note n in c.Notes)
                {
                    midiOut.Send(MidiMessage.StopNote(n.Pitch, 0, 1).RawData);
                }
            }
        }

        /// <summary>
        /// Deprecated, for legacy purposes only
        /// </summary>
        /// <param name="notes"></param>
        public void PlayNotes(IEnumerable<Note> notes)
        {
            foreach (Note n in notes)
            {

                midiOut.Send(MidiMessage.StartNote(n.Pitch, n.Velocity, 1).RawData);
                Thread.Sleep((int)(Note.ToRealDuration(n.Duration)*1000));
                midiOut.Send(MidiMessage.StopNote(n.Pitch, 0, 1).RawData);

            }
        }
        
        /// <summary>
        /// Deprecated, for legacy purposes only
        /// </summary>
        /// <param name="patch"></param>
        /// <param name="channel"></param>
        public void SetPatch(int patch, int channel)
        {
            midiOut.Send(MidiMessage.ChangePatch(patch, channel).RawData);
        }

        public void Close()
        {
            midiOut.Close();
            midiOut.Dispose();
        }
    }
}
