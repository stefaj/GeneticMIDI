using GeneticMIDI.Representation;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticMIDI.Output
{
    public delegate void MusicHandler(object sender, int key, PlaybackMessage msg);
    public class MusicPlayer
    {

        public event MusicHandler OnMessageSent;

        MidiOut midiOut;

        int currentIndex;
        PlaybackInfo currentInfo;
        int state = 0;

        public int MaxKeyTime { get; private set; }

        public int CurrentPosition
        {
            get { return currentIndex; }
        }

        public MusicPlayer()
        {
            midiOut = new MidiOut(0);
            MaxKeyTime = 0;
        }


        public void Stop()
        {
            state = 2;
            midiOut.Reset();
        }

        public void Pause()
        {
            state = 1;
        }

        public void Resume()

        {
            if (currentInfo == null)
                return;
            state = 0;
            var keys = new int[currentInfo.Messages.Keys.Count];
            int i = 0;
            foreach (var k in currentInfo.Messages.Keys)
            {
                keys[i++] = k;
            }
            for (i = currentIndex; i < keys.Length - 1; i++)
            {
                currentIndex = i;
                if (state != 0)
                    break;
                foreach (PlaybackMessage message in currentInfo.Messages[keys[i]])
                {
                    if (OnMessageSent != null)
                        OnMessageSent(this, keys[i], message);

                    midiOut.Send(message.GenerateMidiMessage().RawData);
                }
                int sleep_dur = keys[i + 1] - keys[i];
                Thread.Sleep(sleep_dur);
            }
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

        public void Play(Track track)
        {
            var info = track.GeneratePlaybackInfo();
            Play(info);
        }


        public void Play(PlaybackInfo info)
        {
            if (info.Messages.Count == 0)
                return;

            currentInfo = info;
            state = 0;
            var keys = new int[info.Messages.Keys.Count];
            int i = 0;
            foreach(var k in info.Messages.Keys)
            {
                keys[i++] = k;
            }

            MaxKeyTime = keys[info.Messages.Keys.Count - 1];

            for(i = 0; i < keys.Length - 1; i++)
            {
                currentIndex = i;
                if (state != 0)
                    break;
                foreach(PlaybackMessage message in info.Messages[keys[i]])
                {
                    if (OnMessageSent != null)
                        OnMessageSent(this, keys[i], message);
                    midiOut.Send(message.GenerateMidiMessage().RawData);
                }
                int sleep_dur = keys[i + 1] - keys[i];
                Thread.Sleep(sleep_dur);
            }

        }


        public void Seek(int key)
        {

            if (currentInfo == null)
                return;
            midiOut.Reset();
            currentIndex = key;

            state = 0;
            var keys = new int[currentInfo.Messages.Keys.Count];
            int i = 0;
            bool keySet = false;
            foreach (var k in currentInfo.Messages.Keys)
            {
                keys[i++] = k;
                if (k > key && !keySet)
                {
                    currentIndex = i-1;
                    keySet = true;
                }
            }
   

            for(i = 0; i < keys.Length - 1; i++)
            {
                if (keys[i] == key) 
                    break;
                if (state != 0)
                    break;
                foreach (PlaybackMessage message in currentInfo.Messages[keys[i]])
                {
                    if (message.Message != PlaybackMessage.PlaybackMessageType.Start && message.Message != PlaybackMessage.PlaybackMessageType.Stop)
                        midiOut.Send(message.GenerateMidiMessage().RawData);
                }
            }

            Resume();

        }


        public void PlayChord(Chord c)
        {
            state = 0;
            //midiOut.Send(MidiMessage.ChangePatch((int)PatchNames.Electric_Piano_2, 1).RawData);
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
            state = 0;
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
            state = 0;
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
