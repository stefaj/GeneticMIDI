using AForge.Genetic;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI
{
    class NoteTree : GPCustomTree
    {
        public NoteTree(NoteGene gene) : base(gene)
        {
            this.root.Gene = gene;
        }
        public List<Note> GenerateNotes()
        {
            return this.root.GenerateNotes();
        }
    }
}
