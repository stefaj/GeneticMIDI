using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    [ProtoContract]
    [Serializable]
    public class ActivityMatrix
    {
        [ProtoMember(1)]
        ActivityColumn[] activityColumns;

        public ActivityColumn[] ActivityColumns
        {
            get
            {
                return activityColumns;
            }
        }

        public ActivityMatrix(ActivityColumn[] columns)
        {
            this.activityColumns = columns;
        }

        /*public void Generate(Composition comp)
        {
            activityMatrix = ActivityColumn.GenerateFromComposition(comp);
        }*/

        public static ActivityMatrix GenerateFromComposition(Composition comp)
        {
            
            int maxKeyTime = 0;
            foreach (Track t in comp.Tracks)
            {
                var mel = t.GetMainSequence() as MelodySequence;
                int dur = (int)(1000 * mel.RealDuration);
                if (dur > maxKeyTime)
                    maxKeyTime = dur;
            }

            int total_columns = maxKeyTime / ActivityColumn.KEYTIME;

            var activityMatrix = new ActivityColumn[total_columns];

            for(int i = 0; i < total_columns; i++)
                activityMatrix[i] = new ActivityColumn(comp.Tracks.Count);

           int track_index = 0;
            foreach(Track t in comp.Tracks)
            {
                var mel = t.GetMainSequence() as MelodySequence;

                var notes = mel.ToArray();
                
                int time = 0;
                int note_index = 0;
                foreach(Note n in notes)
                {
                    time += (int)(n.RealDuration * 1000);
                    while(time > note_index * ActivityColumn.KEYTIME)
                    {
                        if (note_index >= total_columns)
                            break;


                        if(n.Pitch >= 0 && n.Velocity > 0)
                            activityMatrix[note_index].Set(track_index);

                        note_index++;

                    }
                    
                }

                track_index++;
                
            }

            return new ActivityMatrix(activityMatrix);
            
        }

        public bool IsActive(int key, int track)
        {
            int index = key / ActivityColumn.KEYTIME;
            return activityColumns[index].IsActive(track);
        }

        public string[] ToStrings()
        {
            string[] ons = new string[this.ActivityColumns[0].Rows];
            for (int i = 0; i < this.ActivityColumns.Length; i++)
            {
                for (int j = 0; j < this.ActivityColumns[i].Rows; j++)
                {
                    bool isActive = this.ActivityColumns[i].IsActive(j);
                    ons[j] += isActive ? '1' : '0';
                }
            }
            return ons;
        }

        public void Save(string path)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter("test.txt");
            var ons = ToStrings();
            foreach (var s in ons)
                writer.WriteLine(s);
            writer.Close();
        }
    }
}
