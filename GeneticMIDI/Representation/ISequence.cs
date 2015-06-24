using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public interface ISequence
    {
        
        int Duration { get; }

        int Length { get; }

        SortedDictionary<int, IEnumerable<PlaybackMessage>> GeneratePlaybackData(byte channel, int time = 0);
    }
}
