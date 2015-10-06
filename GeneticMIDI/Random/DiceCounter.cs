using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Fractal
{
    public class DiceCounter : IDice
    {

        List<IDice> diceList;
        int maxRollCounter;
        int maxRollableValue;

        int currentRollCounter = 0;

        public DiceCounter(int rangeSize, int numberOfDice, Random gen)
        {
            diceList = DiceUtil.GetDice(rangeSize, numberOfDice, gen);
            maxRollCounter = 1 << numberOfDice;
            maxRollableValue = rangeSize;
        }


        public void Roll()
        {
            int oldRollCounter = currentRollCounter;
            currentRollCounter = Increment(currentRollCounter);

            int[] diceToRoll = DiceUtil.getFlippedBits(oldRollCounter, currentRollCounter);

            foreach (int dieNum in diceToRoll)
            {
                diceList[dieNum].Roll();
            }
        }

        public int Get()
        {
            int sum = 0;
            foreach (IDice die in diceList)
            {
                sum += die.Get();
            }
            return sum;
        }

        public int MaximumRollableValue()
        {
            return maxRollableValue;
        }

        private int Increment(int currVal)
        {
            return (currVal + 1) % maxRollCounter;
        }
    }

}
