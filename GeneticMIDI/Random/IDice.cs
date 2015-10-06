using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Fractal
{
    public interface IDice
    {
        void Roll();

        int Get();

        int MaximumRollableValue();
    }
}
