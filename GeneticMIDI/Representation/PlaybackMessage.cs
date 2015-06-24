using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{

    public class PlaybackMessage
    {
        public enum PlaybackMessageType { Start, Stop };

        public PlaybackMessageType Message { get; private set; }
        public byte Channel { get; private set; }
        public byte Velocity { get; private set; }
        public byte Pitch { get; private set; }

        public PlaybackMessage(PlaybackMessageType message, byte channel, byte velocity, byte pitch)
        {
            this.Message = message;
            this.Channel = channel;
            this.Velocity = velocity;
            this.Pitch = pitch;
        }

        public MidiMessage GenerateMidiMessage()
        {
            if (Message == PlaybackMessageType.Start)
                return MidiMessage.StartNote(Pitch, Velocity, Channel);
            else
                return MidiMessage.StopNote(Pitch, 0, Channel);
        }
    }
}
