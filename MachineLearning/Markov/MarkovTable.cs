using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetLearn.Markov
{
    [Serializable]
    [ProtoContract]
    public class MarkovTable<T> where T : IEquatable<T>
    {

        [ProtoMember(1)]
        Dictionary<ChainState<T>, Dictionary<T, int>> frequencyTable;


        [ProtoMember(2)]
        public int Order { get; private set; }
     
        public MarkovTable()
        {
            this.Order = 2;
            this.frequencyTable = new Dictionary<ChainState<T>, Dictionary<T, int>>();
        }

        public MarkovTable(int order)
        {
            this.Order = order; 
            this.frequencyTable = new Dictionary<ChainState<T>, Dictionary<T, int>>();
        }

        public void Add(IEnumerable<T> items_input, IEnumerable<T> items_output)
        {
            //int i = 0;

            List<T> items_list_input = new List<T>();
            List<T> items_list_output = new List<T>();

            foreach (var it in items_input)
                items_list_input.Add(it);
            foreach (var it in items_output)
                items_list_output.Add(it);

            T[] items_arr_input = items_list_input.ToArray();
            T[] items_arr_output = items_list_output.ToArray();
            for (int i = Order; i < items_arr_input.Length; i++)
            {
     
                T[] temp_arr = new T[Order];
                for(int j = Order; j > 0; j--)
                {
                    temp_arr[Order - j] = items_arr_input[i - (j)];
                    //key.Add(items_arr[i - (j)]);
                }
                ChainState<T> key = new ChainState<T>(temp_arr);

                var output_item = items_arr_input[i];


                if (frequencyTable.ContainsKey(key))
                {
                }
                else
                {
                    frequencyTable[key] = new Dictionary<T, int>();
                }
                if (frequencyTable[key].ContainsKey(output_item))
                {
                    frequencyTable[key][output_item]++;
                }
                else
                {
                    frequencyTable[key][output_item] = 1;
                }

            }

        }

        /// <summary>
        /// Returns a random item according to the proportions
        /// </summary>
        /// <typeparam name="B"></typeparam>
        /// <param name="items">Array of values</param>
        /// <param name="proportions">Weight of each item</param>
        /// <param name="rand">Uses own random if none is given</param>
        /// <returns></returns>
        public static B RouletteSelection<B>(B[] items, float[] proportions, Random rand=null)
        {
     
            if (items.Length != proportions.Length)
                throw new Exception("WTP length mismatch");

            float total_freq = 0;
            foreach (var freq in proportions)
                total_freq += freq;

            // Cumulative probabilities
            float[] probabilities_cum = new float[proportions.Length];
            for (int i = 0; i < items.Length; i++ )
            {
                float proportion = proportions[i] / total_freq;

                if (i == 0)
                    probabilities_cum[i] = proportion;
                else
                    probabilities_cum[i] = probabilities_cum[i - 1] + proportion;
            }

            float randomDouble = (float)(rand == null ? StaticRandom.NextDouble() : rand.NextDouble());
            float random_prob = (float)(probabilities_cum[probabilities_cum.Length - 1] * randomDouble);
            int index = Array.BinarySearch<float>(probabilities_cum, random_prob);
            if (index < 0)
                index = -(index + 1);
            if (index == items.Length)
                index = items.Length - 1;
            return items[index];
        }


        public T[] Chain(T[] start, int seed=0)
        {
            
            List<T> ze_items = new List<T>();

            Random random = new Random(seed);

            for (int i = 0; i < start.Length - Order; i++ )
            {
                int max_key_items = Order;
                Queue<T> last_words = new Queue<T>();
                for(int j = 0; j < Order; j++)
                {
                    if (max_key_items > 0)
                    {
                        last_words.Enqueue(start[j+i]);
                        //ze_items.Add(start[j + i]);
                        max_key_items--;
                    }
                    else
                        break;
                }
                ChainState<T> item_key = new ChainState<T>(last_words);

                if (!frequencyTable.ContainsKey(item_key))
                {
                    if (!typeof(T).GetInterfaces().Contains(typeof(IDifference<T>)))
                        continue;
                    double smallestDistance = double.MaxValue - 1;
                     var smallest_key = frequencyTable.Keys.First();
                    foreach(var key in frequencyTable.Keys)
                    {
                        double diff = key.Difference(item_key);
                        if(diff < smallestDistance)
                        {
                            smallestDistance = diff;
                            smallest_key = key;
                        }
                    }
                    item_key = smallest_key;
                    //  continue;
                    // Rather get closest key
                    
                }
                   

                T[] words_arr = frequencyTable[item_key].Keys.ToArray();

                float[] frequencies = new float[words_arr.Length];

                int k = 0;
                foreach (var next_word in words_arr)
                {
                    frequencies[k++] = frequencyTable[item_key][next_word];
                }

                T next_word_ = RouletteSelection<T>(words_arr, frequencies, random);
                ze_items.Add(next_word_);
            }
            
            return ze_items.ToArray();
        }
    }
}
