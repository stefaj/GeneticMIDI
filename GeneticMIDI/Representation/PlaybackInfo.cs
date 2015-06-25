using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class PlaybackInfo
    {
        public SortedDictionary<int, List<PlaybackMessage>> Messages {get; private set;}

        public PlaybackInfo()
        {
            Messages = new SortedDictionary<int, List<PlaybackMessage>>();
        }

        public void Add(int time, PlaybackMessage message)
        {
            if (!Messages.ContainsKey(time))
                Messages[time] = new List<PlaybackMessage>() { message };
            else
                Messages[time].Add(message);
        }

        public void Add(int time, IEnumerable<PlaybackMessage> messages)
        {
            foreach (PlaybackMessage m in messages)
                Add(time, m);
        }

        public static PlaybackInfo operator +(PlaybackInfo info1, PlaybackInfo info2)
        {
            PlaybackInfo info = new PlaybackInfo();
            foreach (int k in info1.Messages.Keys)
                info.Add(k, info1.Messages[k]);
            foreach (int k in info2.Messages.Keys)
                info.Add(k, info2.Messages[k]);
            return info;
        }

    }
}
