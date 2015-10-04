using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics
{
    public abstract class MetricFrequency : IMetric
    {
        private Dictionary<Pair, float> frequencies;
        protected const float p = 0.02f;

        public MetricFrequency()
        {
            frequencies = new Dictionary<Pair, float>();
        }
        public Dictionary<Pair, float> Frequencies
        {
            get { return frequencies; }
        }

        public Dictionary<Pair, float> Generate(Note[] notes)
        {
            frequencies = new Dictionary<Pair, float>();
            frequencies.Clear();
            GenerateFrequencies(notes);
            Filter();
            Sort();
            //Sort
            return frequencies;
        }

        public void Filter()
        {
            int N = frequencies.Keys.Count;
            var keys = frequencies.Keys.ToArray();
            for (int i = 0; i < N; i++ )
            {
                var k = keys[i];
                if (!frequencies.ContainsKey(k))
                    continue;
                if (frequencies[k] / (float)N < p)
                {
                    frequencies.Remove(k);
                }
            }
        }

        private void Sort()
        {
            var list = frequencies.Keys.ToList();
            list.Sort();

            Dictionary<Pair, float> frequencies2 = new Dictionary<Pair,float>();
            
            // Loop through keys.
            foreach (var key in list)
            {
                frequencies2[key] = frequencies[key];
            }

            frequencies = frequencies2;
        }

        public void Add(Pair p)
        {
            if (float.IsNaN(p.Comp1) || float.IsNaN(p.Comp2) || float.IsInfinity(p.Comp1) || float.IsInfinity(p.Comp2))
                return;
            p.Comp1 = (float)Math.Round(p.Comp1, 2);
            p.Comp2 = (float)Math.Round(p.Comp2, 2);

            if (frequencies.ContainsKey(p))
                frequencies[p]++;
            else
                frequencies[p] = 1;
        }

        public abstract void GenerateFrequencies(Note[] notes);
    }
}
