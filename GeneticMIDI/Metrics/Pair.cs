using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics
{
   public class Pair : IComparable 
    {
       public float Comp1;
       public float Comp2;
       public bool IsSingle { get; private set; }
       public Pair(float v1, float v2=0, bool isSingle=false)
       {
           this.Comp1 = v1;
           this.Comp2 = v2;
           this.IsSingle = isSingle;
       }

       public override bool Equals(object obj)
       {
           var p = obj as Pair;
           if (p == null)
               return false;
           return p.Comp1 == this.Comp1 && p.Comp2 == this.Comp2;
       }
       public override int GetHashCode()
       {
           return (int)(3571 * Comp2 + 3433 * Comp1);
       }

       public override string ToString()
       {
           if (IsSingle)
               return Comp1.ToString();
           else
               return "(" + Comp1 + "," + Comp2 + ")";
       }

       public int CompareTo(object obj)
       {
           Pair p = obj as Pair;
           if (p == null)
               throw new Exception("Cannot compare to a stranger");

           int comp = this.Comp1.CompareTo(p.Comp1);
           if (comp == 0)
               return this.Comp2.CompareTo(p.Comp2);
           return comp;
       }
    }
}
