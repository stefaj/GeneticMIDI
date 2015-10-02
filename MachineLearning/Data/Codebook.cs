using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetLearn.Data
{
    public class Codebook<T>
    {
        public int TotalUniqueSymbols
        {
            get
            {
                return this.hashes.Keys.Count;
            }
        }
        private int hash;
        private Dictionary<T, int> hashes = new Dictionary<T, int>();
        public Codebook()
        {
            hashes = new Dictionary<T, int>();
            hash = 0;
        }

        public void Add(IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                if (!hashes.ContainsKey(item))
                    hashes[item] = hash++;
            }
        }


        public int[] ToCodes(IEnumerable<T> items)
        {
            List<int> hashedNotes = new List<int>();
            foreach (var n in items)
            {
                if (!hashes.ContainsKey(n))
                    hashes[n] = hash++;
                hashedNotes.Add(hashes[n]);
            }
            return hashedNotes.ToArray();
        }

        public static Dictionary<A, B> ReverseDictionary<A, B>(Dictionary<B, A> dictionary)
        {
            Dictionary<A, B> reverse_note_map = new Dictionary<A, B>();

            foreach (var k in dictionary.Keys)
                reverse_note_map[dictionary[k]] = k;
            return reverse_note_map;
        }

        public T[] Translate(IEnumerable<int> hashes)
        {
            List<T> items = new List<T>();
            var reversedHashes = ReverseDictionary(this.hashes);
            foreach(var item in hashes)
            {
                if (reversedHashes.ContainsKey(item))
                    items.Add(reversedHashes[item]);
            }
            return items.ToArray();
        }

        public T TranslateSingle(int hash)
        {
            var reversedHashes = ReverseDictionary(this.hashes);
            if (reversedHashes.ContainsKey(hash))
                return (reversedHashes[hash]);
            throw new Exception("Hash not in dictionary");
        }
    }
}
