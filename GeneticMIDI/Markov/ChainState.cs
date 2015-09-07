﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTestEnvironment
{
    [Serializable]
    [ProtoContract]
    public class ChainState<T> : IEquatable<ChainState<T>>
    {
        [ProtoMember(1)]
        T[] items;

        public ChainState()
        {
            items = new T[0];
        }

        public ChainState(params T[] items)
        {
            this.items = items;
        }

        public ChainState(IEnumerable<T> items)
        {
            var length = items.Count();

            int i = 0;
            this.items = new T[length];
            foreach(var item in items)
            {
                this.items[i++] = item;
            }            
        }

        public T[] GetItems()
        {
            return items;
        }


        public bool Equals(ChainState<T> other)
        {
            if (this.items.Length != other.items.Length)
                return false;
            for(int i = 0; i < this.items.Length; i++)
            {
                if (!this.items[i].Equals(other.items[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var code = this.items.Length.GetHashCode();

            for (int i = 0; i < this.items.Length; i++)
            {
                code ^= this.items[i].GetHashCode();
            }

            return code;
        }
    }
}
