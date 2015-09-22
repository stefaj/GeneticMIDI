using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.ML
{
    public class Sample
    {
        public double[] Inputs { get; private set; }
        public double[] Outputs { get; private set; }

        public int GetInputsCount
        {
            get
            {
                return Inputs.Length;
            }
        }

        public int GetOutputsCount
        {
            get
            {
                return Outputs.Length;
            }
        }

        public Sample()
        {

        }

        public Sample(double[] inputs, double[] outputs)
        {
            this.Inputs = inputs;
            this.Outputs = outputs;
        }
    }
}
