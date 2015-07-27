using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class ActivityColumn : IEquatable<ActivityColumn>
    {
        bool[] activities;
        public const int KEYTIME = 100;

        public int Rows
        {
            get
            {
                return activities.Length;
            }
        }

        public void Set(int index)
        {
            activities[index] = true;
        }

        public void Unset(int index)
        {
            activities[index] = false;
        }

        public bool IsActive(int index)
        {
            return activities[index];
        }

        public ActivityColumn(int rows)
        {
            activities = new bool[rows];
        }

        public static ActivityColumn[] GenerateFromComposition(Composition comp)
        {
            List<ActivityColumn> columns = new List<ActivityColumn>();

            int maxKeyTime = 0;
            Dictionary<int, int> melody_time = new Dictionary<int, int>();
            Dictionary<int, int> melody_indices = new Dictionary<int, int>();
            int i = 0;
            foreach(Track t in comp.Tracks)
            {
                var mel = t.GetMainSequence() as MelodySequence;
                int dur = (int)(1000 * mel.RealDuration);
                
                if (dur > maxKeyTime)
                    maxKeyTime = dur;
                melody_time[i] = 0;
                melody_indices[i] = 0;
                i++;
            }
            
            
            for (i = KEYTIME; i < maxKeyTime; i += KEYTIME)
            {
                ActivityColumn column = new ActivityColumn(comp.Tracks.Count);

                int j = 0;
                foreach(Track t in comp.Tracks)
                {
                    var mel = t.GetMainSequence() as MelodySequence;
                    var notes = mel.ToArray();
                    if (melody_indices[j] >= notes.Length)
                        continue;

                    if (melody_time[j] >= i - KEYTIME && melody_time[j] <= i)
                        column.Set(j);
                    columns.Add(column);

                    while(melody_time[j] <= i)
                    {
                        melody_time[j] += (int)(notes[melody_indices[j]].RealDuration*1000);
                        melody_indices[j]++;
                    }
                    

                    j++;
                }     
            }

            return columns.ToArray();

        }

        public bool Equals(ActivityColumn other)
        {
            if (other == null)
                return false;

            ActivityColumn most = null;
            ActivityColumn least = null;
            int most_rows = 0;
            int least_rows = 0;
            if(this.Rows > other.Rows)
            {
                most = this;
                least = other;
                most_rows = this.Rows;
                least_rows = other.Rows;
            }
            else
            {
                least = this;
                most = other;
                most_rows = other.Rows;
                least_rows = this.Rows;
            }

            for(int i = 0; i < most_rows; i++)
            {
                bool other_set = false;
                if (i < least_rows)
                    other_set = other.IsActive(i);
                bool this_set = this.IsActive(i);

                if (this_set != other_set)
                    return false;
            }
            return true;            
        }
    }
}
