using GeneticMIDI.Output;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visualizer
{
    public class MusicPlayer
    {
        enum PlayingState { Playing, Paused, Stopped };

        PlayingState state;

        GeneticMIDI.Output.MusicPlayer player;

        System.Threading.Thread playingThread;

        public event MusicHandler OnMessageSent;

        public int MaxKey 
        { 
            get 
            { 
                if(player == null)
                    return 0;
                return player.MaxKeyTime;
            } 
        }

        public MusicPlayer()
        {
            player = new GeneticMIDI.Output.MusicPlayer();
            player.OnMessageSent += player_OnMessageSent;
        }

        void player_OnMessageSent(object sender, int key, PlaybackMessage msg)
        {
            if (OnMessageSent != null)
                OnMessageSent(this, key, msg);
        }

        public void Play(MelodySequence seq, PatchNames instrument = PatchNames.Acoustic_Grand)
        {
            if (playingThread != null)
                playingThread.Abort();
            player.SetPatch((int)instrument, 1);
            playingThread = new System.Threading.Thread(() => player.Play(seq));
            playingThread.Start();

        }

        public void Play(Composition comp)
        {
            if (playingThread != null)
                playingThread.Abort();
            playingThread = new System.Threading.Thread(() => player.Play(comp));
            playingThread.Start();
        }

        public void Play(PlaybackInfo info)
        {
            if (playingThread != null)
                playingThread.Abort();
            playingThread = new System.Threading.Thread(() => player.Play(info));
            playingThread.Start();
        }

        public void Seek(int key)
        {
            player.Pause();
            if (playingThread != null)
                playingThread.Abort();
            playingThread = new System.Threading.Thread(() => player.Seek(key));
            playingThread.Start();
            
        }

        public void Stop()
        {
            if (playingThread == null)
                return;

            player.Stop();
            System.Threading.Thread.Sleep(100);
            playingThread.Abort();
        }

        public void Pause()
        {
            player.Pause();
        }
    }
}
