using DotNetLearn.Markov;
using DotNetLearn.Statistics;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    public delegate void ProgressUpdate(int curIndex, int maxIndex, double fitness);
    public class SamplingWithReplacement : INoteGenerator
    {

        MarkovChain<Note> model;
        CompositionCategory category;

        SimpleRNG random;

        int count;
        double averagePitch;
        double stdPitch;

        public event ProgressUpdate OnProgressChange;

        public MelodySequence OriginalSequence { get; private set; }

        PatchNames instrument;

        public int MaxLength { get; private set; }

        public int MaxIterations { get; set; }

        public SamplingWithReplacement(CompositionCategory cat, PatchNames instrument)
        {
            this.MaxIterations = 200;
            this.MaxLength = 100;
            this.instrument = instrument;
            this.category = cat;
            random = new SimpleRNG();
            random.SetSeedFromSystemTime();
            model = new MarkovChain<Note>(1);
            foreach(var comp in cat.Compositions)
            {
                foreach(var track in comp.Tracks)
                {
                    if (!IsValidTrack(track))
                        continue;

                    var mel = (track.GetMainSequence() as MelodySequence).Clone() as MelodySequence;
                    mel.StandardizeDuration();
                    model.Add(mel.ToArray());

                    break;//only first track
                }
            }

            CalculateStats();
        }

        private void CalculateStats()
        {
            count = 0;
            double sum = 0;
            foreach (var comp in category.Compositions)
            {
                foreach(var track in comp.Tracks)
                {
                    if (track.Channel == 10)
                        continue;
                    var mel = track.GetMainSequence() as MelodySequence;

                    foreach(var note in mel.Notes)
                    {
                        count++;
                        sum += note.Pitch;
                    }
                }
            }

            averagePitch = sum / count;

            sum = 0;
            foreach (var comp in category.Compositions)
            {
                foreach (var track in comp.Tracks)
                {
                    if (track.Channel == 10)
                        continue;
                    var mel = track.GetMainSequence() as MelodySequence;

                    foreach (var note in mel.Notes)
                    {
                        sum += Math.Pow(note.Pitch - averagePitch, 2);
                    }
                }
            }

            stdPitch = Math.Sqrt(sum / count);
        }

        private MelodySequence GetRandomMelody()
        {
            var comp = category.Compositions[random.Next(category.Compositions.Length)];
            var track = comp.Tracks[random.Next(comp.Tracks.Count)];

            // only first track
            track = comp.Tracks[0];

            if(!IsValidTrack(track))
                return GetRandomMelody();
                
            var mel = (track.GetMainSequence() as MelodySequence).Clone() as MelodySequence;

            mel.StandardizeDuration();

            return mel;
        }

        private bool IsValidTrack(Track track)
        {
            if (track.Channel == 10)
                return false;
            var mel = track.GetMainSequence() as MelodySequence;
            if (mel.Length > MaxLength)
                return false;

            return true;
        }



        public MelodySequence Generate()
        {
            var mel = GetRandomMelody();
            OriginalSequence = mel;
            var notes = mel.ToArray();

            var possibleDurations = Enum.GetValues(typeof(Durations)).Cast<Durations>().ToArray();

            Note[] newNotes = new Note[notes.Length];
            Array.Copy(notes, newNotes, notes.Length);

            double originalProb = model.EvaluateLogProbability(notes);
            double prevProb = originalProb;

            int minPitch = (int)(averagePitch - stdPitch); if (minPitch < 0) minPitch = 0;
            int maxPitch = (int)(averagePitch + stdPitch); if (maxPitch > 127) maxPitch = 127;

            var possibleNotes = model.GetPossibleItemClasses();

            int iter = 0;
            //for (int i = 0; i < notes.Length; i++)
            while(true)
            {
                int i = random.Next(notes.Length);

                Console.WriteLine("Progress: {0} / {1}  --  OldProb {2}, NewProb {3}", iter, MaxIterations, originalProb, prevProb);
                foreach (Note newNote in possibleNotes)
                {
                    //var newNote = new Note((int)pitch, (int)dur);

                    Note[] tempNotes = new Note[notes.Length];
                    Array.Copy(newNotes, tempNotes, notes.Length);

                    tempNotes[i] = newNote;

                    double newProb = model.EvaluateLogProbability(tempNotes);
                    if (newProb >= prevProb)
                    {
                        prevProb = newProb;
                        newNotes[i] = newNote;
                    }
                }
                if (iter >= MaxIterations)
                    break;

                if (OnProgressChange != null)
                    OnProgressChange(iter, MaxIterations, prevProb);

                iter++;
            }
 

            double finalProb = model.EvaluateLogProbability(newNotes);

            return new MelodySequence(newNotes);
        }


        public MelodySequence Next()
        {
            return Generate();
        }

        public bool HasNext
        {
            get { return true; }
        }

        public PatchNames Instrument
        {
            get { return instrument; }
        }
    }
}
