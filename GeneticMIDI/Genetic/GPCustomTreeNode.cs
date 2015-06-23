// AForge Genetic Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2006-2009
// andrew.kirillov@aforgenet.com
//

namespace AForge.Genetic
{
    using GeneticMIDI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

    /// <summary>
    /// Represents tree node of genetic programming tree.
    /// </summary>
    /// 
    /// <remarks><para>In genetic programming a chromosome is represented by a tree, which
    /// is represented by <see cref="GPTreeChromosome"/> class. The <see cref="GPTreeNode"/>
    /// class represents single node of such genetic programming tree.</para>
    /// 
    /// <para>Each node may or may not have children. This means that particular node of a genetic
    /// programming tree may represent its sub tree or even entire tree.</para>
    /// </remarks>
    /// 
    public class GPCustomTreeNode : ICloneable
    {
        /// <summary>
        /// Gene represented by the chromosome.
        /// </summary>
        public IGPGene Gene;

        /// <summary>
        /// List of node's children.
        /// </summary>
        public List<GPCustomTreeNode> Children;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPTreeNode"/> class.
        /// </summary>
        /// 
        internal GPCustomTreeNode() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GPTreeNode"/> class.
        /// </summary>
        /// 
        public GPCustomTreeNode(IGPGene gene)
        {
            Gene = gene;
        }

        /// <summary>
        /// Get string representation of the node.
        /// </summary>
        /// 
        /// <returns>Returns string representation of the node.</returns>
        /// 
        /// <remarks><para>String representation of the node lists all node's children and
        /// then the node itself. Such node's string representations equals to
        /// its reverse polish notation.</para>
        /// 
        /// <para>For example, if nodes value is '+' and its children are '3' and '5', then
        /// nodes string representation is "3 5 +".</para>
        /// </remarks>
        /// 
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Children != null)
            {
                // walk through all nodes
                foreach (GPCustomTreeNode node in Children)
                {
                    sb.Append(node.ToString());
                }
            }

            // add gene value
            sb.Append(Gene.ToString());
            sb.Append(" ");

            return sb.ToString();
        }

        /// <summary>
        /// Clone the tree node.
        /// </summary>
        /// 
        /// <returns>Returns exact clone of the node.</returns>
        /// 
        public object Clone()
        {
            GPCustomTreeNode clone = new GPCustomTreeNode();

            // clone gene
            clone.Gene = this.Gene.Clone();
            // clone its children
            if (this.Children != null)
            {
                clone.Children = new List<GPCustomTreeNode>();
                // clone each child gene
                foreach (GPCustomTreeNode node in Children)
                {
                    clone.Children.Add((GPCustomTreeNode)node.Clone());
                }
            }
            return clone;
        }

        public List<Note> GenerateNotes()
        {
            
            var g = Gene as NoteGene;
            if (this.Children == null)
            {
                //No choice, no children, you gonna have to be an argument
                g.GeneType = GPGeneType.Argument;
                return new List<Note>() { g.GenerateNote() };
            }
            if (Gene.GeneType == GPGeneType.Argument)
                return new List<Note>() { g.GenerateNote() };
            else
            {
                switch (g.Function)
                {
                    case NoteGene.FunctionTypes.Series:
                        var childnotes = this.Children[0].GenerateNotes();
                        childnotes.AddRange(this.Children[1].GenerateNotes());
                        return childnotes;
                    /*case NoteGene.FunctionTypes.LengthScale:
                        var notes1 = this.Children[0].GenerateNotes(); //Note to be modified
                        foreach(Note note in notes1)
                        {
                            float arg = (this.Children[1].Gene as NoteGene).Pitch; //Argument
                            arg -= 6; arg /= 6;
                            note.Duration = (int)(note.Duration * (1+arg));
                        }
                        return notes1;*/
                    case NoteGene.FunctionTypes.PitchScale:
                        var notes2 = this.Children[0].GenerateNotes();
                        float arg2 = (this.Children[1].Gene as NoteGene).Pitch; //Argument
                        foreach (Note note in notes2)
                        {
                            arg2 -= 6; arg2 /= 6;
                            note.Duration = (int)(note.Duration * (1 + arg2));
                        }
                        return notes2;
                    default:
                        throw new Exception("Eh wena");
                }
            }
        }
    }
}
