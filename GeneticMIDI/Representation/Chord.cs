using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class Chord
    {
        public int Duration { get; private set; }
        public int Velocity { get; private set; }
        public Note[] Notes { get; private set; }

        public int Length { get { return Notes.Length; } }

        public Chord(int[] pitches, int duration, int velocity=127 )
        {
            this.Duration = duration;
            this.Velocity = velocity;
            this.Notes = new Note[pitches.Length];
            for(int i = 0; i < pitches.Length; i++)
            {
                Notes[i] = new Note(pitches[i], duration, velocity);
            }
        }

        public Chord(NoteNames[] notes, int octave, Durations duration, int velocity=127)
        {
            this.Duration = (int)duration;
            this.Velocity = velocity;
            this.Notes = new Note[notes.Length];
            for (int i = 0; i < notes.Length; i++)
            {
                Notes[i] = new Note(notes[i], octave, Duration, velocity);
            }
        }

        public void Transpose(int ht)
        {

        }
    }
}
