using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Fractal
{
    class SidedDice : IDice
    {

        Random randomSelector;
        int numSides;
        int currentValue = 0;

        public SidedDice(int numberOfSides, Random randGen)
        {
            numSides = numberOfSides;
            randomSelector = randGen;
            Roll();
        }


        public void Roll()
        {
            if (numSides != 0)
            {
                currentValue = randomSelector.Next(numSides);
            }
        }


        public int Get()
        {
            return currentValue;
        }


        public int MaximumRollableValue()
        {
            return numSides - 1;
        }


    }
}
