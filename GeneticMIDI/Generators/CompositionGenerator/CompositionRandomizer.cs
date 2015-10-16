using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.CompositionGenerator
{
    public class CompositionRandomizer :  ICompositionGenerator
    {
        public event EventHandler OnCompositionChange;

        Composition activeComposition;

        List<INoteGenerator> generators;

        int channelIndex = 1;

        public Composition ActiveComposition
        {
            get
            {
                return activeComposition;
            }
            set
            {
                activeComposition = value;
                if (OnCompositionChange != null)
                    OnCompositionChange(this, new EventArgs());
            }
        }

        public CompositionRandomizer()
        {
            ActiveComposition = new Composition();
            generators = new List<INoteGenerator>();
            this.channelIndex = 1;
        }
               

        public Composition GenerateComposition()
        {
           return ActiveComposition;
        }
        
        /// <summary>
        /// Adds a track and returns the reference
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="gen"></param>
        /// <returns>Reference of added track</returns>
        public Track Add(MelodySequence seq, INoteGenerator gen)
        {
            Track t = new Track(gen.Instrument, (byte)channelIndex++);
            t.AddSequence(seq);
            ActiveComposition.Tracks.Add(t);
            generators.Add(gen);
            
            if (OnCompositionChange != null)
                OnCompositionChange(this, new EventArgs());

            return t;
        }

        public void Remove(Track t)
        {
            int index = ActiveComposition.Tracks.IndexOf(t);
            Remove(index);
        }

        public void Remove(int index)
        {
            ActiveComposition.Tracks.RemoveAt(index);
            generators.RemoveAt(index);

            if (OnCompositionChange != null)
                OnCompositionChange(this, new EventArgs());
        }

        public void SetCompositionTrack(int i, Track t)
        {
            ActiveComposition.Tracks[i] = t;

            if (OnCompositionChange != null)
                OnCompositionChange(this, new EventArgs());
        }


        public void Clear()
        {
            this.channelIndex = 1;
            this.generators.Clear();
            this.ActiveComposition = new Composition();

            if (OnCompositionChange != null)
                OnCompositionChange(this, new EventArgs());
        }
      
        public void Next()
        {
            ActiveComposition = new Composition();
            int i = 1;
            foreach(var gen in generators)
            {
                Track t = new Track(gen.Instrument, (byte)i++);
                MelodySequence seq = gen.Next();
                t.AddSequence(seq);
                ActiveComposition.Add(t); 
            }
        }

    }
}
