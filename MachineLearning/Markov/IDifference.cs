using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetLearn.Markov
{
    public interface IDifference<T>
    {
        double GetDifference(T other);
    }
}
