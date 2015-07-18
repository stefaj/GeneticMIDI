using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{

    public struct PlaybackMessage
    {
        public enum PlaybackMessageType { Start, Stop, Patch, Control };

        PlaybackMessageType message;
        public PlaybackMessageType Message { get {return message;} }

        byte channel;
        public byte Channel { get { return channel; } }

        byte velocity;
        public byte Velocity { get { return velocity; } }

        byte pitch;
        public byte Pitch { get { return pitch; } }

        // Only really used for start messages
        int duration;
        public int Duration { get { return duration; } }

        public PlaybackMessage(PlaybackMessageType message_, byte channel_, byte velocity_, byte pitch_, int duration_=0)
        {
            message = message_;
            channel = channel_;
            velocity = velocity_;
            pitch = pitch_;
            duration = duration_;
        }

        public PlaybackMessage(PatchNames patch, byte channel_)
        {
            message = PlaybackMessageType.Patch;
            velocity = (byte)patch;
            channel = channel_;
            pitch = 0;
            duration = 0;

        }

        public MidiMessage GenerateMidiMessage()
        {
            switch (Message)
            {
                case PlaybackMessageType.Start:
                    return MidiMessage.StartNote(Pitch, Velocity, Channel);
                case PlaybackMessageType.Stop:
                    return MidiMessage.StopNote(Pitch, 0, Channel);
                case PlaybackMessageType.Patch:
                    return MidiMessage.ChangePatch(Velocity, Channel);
                case PlaybackMessageType.Control:
                    return MidiMessage.ChangeControl(0,0,0);
            }
            throw new Exception("Not a valid message");

        }
    }
}
