using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public class ActivityMatrix
    {
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
    }
}
