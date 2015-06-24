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
            var msgs = seq.GeneratePlaybackData(1,0);
            Play(msgs, 0);
        }

        private void Play(SortedDictionary<int, IEnumerable<PlaybackMessage>> messages, int t0=0)
        {
            var keys = new int[messages.Keys.Count];
            int i = 0;
            foreach(var k in messages.Keys)
            {
                keys[i++] = k;
            }
            for(i = 0; i < keys.Length - 1; i++)
            {
                foreach(PlaybackMessage message in messages[keys[i]])
                {
                    midiOut.Send(message.GenerateMidiMessage().RawData);
                }
                int sleep_dur = keys[i + 1] - keys[i];
                Thread.Sleep(sleep_dur);
            }

        }

        private SortedDictionary<int, IEnumerable<PlaybackMessage>> Merge(SortedDictionary<int, IEnumerable<PlaybackMessage>>[] messages)
        {
            SortedDictionary<int, IEnumerable<PlaybackMessage>> main_dic = new SortedDictionary<int, IEnumerable<PlaybackMessage>>();
            foreach(var d in messages)
            {
                foreach(int k in d.Keys)
                {
                    if(!main_dic.ContainsKey(k))
                    {
                        main_dic[k] = d[k];
                    }
                    else
                    {
                        int j = 0;
                        PlaybackMessage[] msgs = new PlaybackMessage[main_dic[k].Count() +d[k].Count()];
                        foreach(PlaybackMessage m in main_dic[k])
                            msgs[j++] = m;
                        foreach(PlaybackMessage m in d[k])
                            msgs[j++] = m;
                        main_dic[k] = msgs;
                    }
                }
            }
            return main_dic;
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
