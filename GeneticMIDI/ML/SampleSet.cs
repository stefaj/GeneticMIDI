using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.ML
{
    public class SampleSet : IEnumerable
    {
        List<Sample> samples;

        public int Length
        {
            get
            {
                return samples.Count;
            }
        }

        public Sample this[int i]
        {
            get
            {
                return samples[i];
            }
        }

        public SampleSet()
        {
            this.samples = new List<Sample>();
        }

        public SampleSet(Sample[] samples)
        {
            this.samples = new List<Sample>();
            this.samples.AddRange(samples);
        }

        public void Add(Sample s)
        {
            this.samples.Add(s);
        }

        public void AddAll(Sample[] s)
        {
            this.samples.AddRange(s);
        }

        public void AddAll(SampleSet s)
        {
            this.samples.AddRange(s.samples);
        }

        public IEnumerator GetEnumerator()
        {
            return samples.GetEnumerator();
        }
    }
}
