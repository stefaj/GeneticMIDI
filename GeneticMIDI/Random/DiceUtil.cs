using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Fractal
{
    class DiceUtil
    {

        public static List<IDice> GetDice(int rangeSize, int numberOfDice, Random gen)
        {
            List<IDice> diceList = new List<IDice>(numberOfDice);

            int baseSidesPerDie = ((rangeSize - 1) / numberOfDice) + 1;
            int numberOfDieWithOneMoreSide = ((rangeSize - 1) % numberOfDice) + 1;

            for (int i = 0; i < numberOfDieWithOneMoreSide; i++)
            {
                diceList.Add(new SidedDice(baseSidesPerDie + 1, gen));
            }
            for (int i = numberOfDieWithOneMoreSide; i < numberOfDice; i++)
            {
                diceList.Add(new SidedDice(baseSidesPerDie, gen));
            }

            return diceList;
        }


        /// <summary>
        /// Returns how many bits are flipped
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static int[] getFlippedBits(int val1, int val2)
        {
            int MAX_BITS = (int)(Math.Log(int.MaxValue) / Math.Log(2));

            // XOR the values; the positions that are '1' are what's changed.
            int flipTrack = val1 ^ val2;

            HashSet<int> retVal = new HashSet<int>();
            for (int i = 0; i < MAX_BITS; i++)
            {
                int bitCheckMask = 1 << i;
                if ((flipTrack & bitCheckMask) != 0)
                {
                    retVal.Add(i);
                }
            }
            return retVal.ToArray();
        }
    }
}
