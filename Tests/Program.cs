using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            DotNetLearn.Data.Sample s = new DotNetLearn.Data.Sample(new double[] { 0 }, new double[] { 1 });
            Console.WriteLine(s);
            Console.ReadLine();
        }
    }
}
