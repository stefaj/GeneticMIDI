using AForge;
using AForge.Genetic;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI
{
    class NoteGene : IGPGene 
    {
        public enum FunctionTypes
        {
            Series,
            PitchScale
        }
        public const int FUNCTIONS_NUM = 2;
        public FunctionTypes Function{get; set;}
        public int Pitch { get; private set; }
        public int Octave { get; private set; }
        public int Duration { get; private set; }

        protected static ThreadSafeRandom rand = new ThreadSafeRandom();

        public NoteGene(int pitch, int duration, int octave, FunctionTypes function, GPGeneType type)
        {
            this.Function = function;
            this.GeneType = type;
            this.Octave = octave;
            this.Pitch = pitch;
            this.Duration = duration;
        }

        public NoteGene(GPGeneType type)
        {
            
            Generate(type);
        }

        public int ArgumentsCount
        {
            get 
            {
                if(GeneType == GPGeneType.Function)
                    return 2;
                else return 0;
            }
        }

        public GPGeneType GeneType {get; set;}
       

        public IGPGene Clone()
        {
            var NoteGene = new NoteGene(Pitch, Duration, Octave, Function, GeneType);
            return NoteGene;
        }

        public IGPGene CreateNew(GPGeneType type)
        {
            return new NoteGene(type);
        }

        public IGPGene CreateNew()
        {
            return CreateNew((rand.Next(5) == 1) ? GPGeneType.Argument : GPGeneType.Function);
        }

        public void Generate(GPGeneType type)
        {
            if(type == GPGeneType.Function)
            {
              //  Function = (FunctionTypes)(rand.Next(FUNCTIONS_NUM));
                Function = FunctionTypes.Series;
            }
            this.GeneType = type;
            Pitch = rand.Next(0, 12);
            Octave = rand.Next(4, 7);
            Duration = rand.Next(1,7)*4;
        }

        public void Generate()
        {
            Generate((rand.Next(5) == 1) ? GPGeneType.Argument : GPGeneType.Function);
        }

        public int MaxArgumentsCount
        {
            get { return 2; }
        }

        public Note GenerateNote()
        {
            if (GeneType == GPGeneType.Argument)
                return new Note(this.Pitch + this.Octave*12, (int)(this.Duration));
            else
                throw new Exception("Not an argument");
        }
    }
}
