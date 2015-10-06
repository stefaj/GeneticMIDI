using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Fractal
{
    public abstract class BrownNoteGenerator {
    /**
     * 
     * @param nrr             The restrictor to use for note generation.
     * @param lowPitchChange  The smallest allowed step for pitch changes.
     * @param highPitchChange The largest allowed step for pitch changes.
     * @param lowLengthStep   The smallest allowed step for duration changes.
     * @param highLengthStep  The largest allowed step for duration changes.
     */
    public BrownNoteGenerator(
            NoteRangeRestrictor nrr,
            int lowPitchChange, int highPitchChange, 
            int lowLengthStep, int highLengthStep
            ) {
        restrictor = nrr;
        
        lowPC = lowPitchChange;
        highPC = highPitchChange;
        lowLC = lowLengthStep;
        highLC = highLengthStep;

        // Set first note to midpoint of ranges
        nextPitchIndex = (int) ( restrictor.GetNumPitches() / 2 );
        nextLengthIndex = (int) ( restrictor.GetNumDurations() / 2 );
    }

    /**
     * @return A Note within a fixed number of steps of the previous note,
     * given arguments provided to the constructor.
     * 
     * @throws OutOfMIDIRangeException 
     */
    
    public Note GetNextNote() 
    {
        int currentPitchIndex = nextPitchIndex;
        int currentLengthIndex = nextLengthIndex;
        nextPitchIndex = CalculateChange(
                currentPitchIndex, lowPC, highPC, restrictor.GetNumPitches()
                );
        nextLengthIndex = CalculateChange(
                currentLengthIndex, lowLC, highLC, restrictor.GetNumDurations()
                );
        return new Note(restrictor.GetPitch(currentPitchIndex), 
                restrictor.GetDuration(currentLengthIndex));
    }

    
    /**
     * Calculates the amount of change from the previous note on one parameter
     * (such as pitch or duration), given caps on the size of the step 
     * in each direction.
     * 
     * @param startVal        The numerical representation of the value being changed.
     * @param maxNegChange    The maximum amount startVal can be changed in the negative direction.
     * @param maxPosChange    The maximum amount startVal can be changed in the positive direction.
     * @param maxIndexPlusOne One above the maximum allowable value for the value produced.
     * @return The numerical representation for the new value.
     */
    public abstract int CalculateChange(
            int startVal, int maxNegChange,
            int maxPosChange, int maxIndexPlusOne
            );

    private int nextPitchIndex, nextLengthIndex;

    private int lowPC;
    private int highPC;
    private int lowLC;
    private int highLC;
    
    private NoteRangeRestrictor restrictor;
}

}
