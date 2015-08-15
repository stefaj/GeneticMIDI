using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
     public enum PatchNames
        {
            Acoustic_Grand,Bright_Acoustic,Electric_Grand,Honky_Tonk,Electric_Piano_1,Electric_Piano_2,Harpsichord,Clav,
            Celesta,Glockenspiel,Music_Box,Vibraphone,Marimba,Xylophone,Tubular_Bells,Dulcimer,
            Drawbar_Organ,Percussive_Organ,Rock_Organ,Church_Organ,Reed_Organ,Accoridan,Harmonica,Tango_Accordian,
            Acoustic_Guitarnylon,Acoustic_Guitarsteel,Electric_Guitarjazz,Electric_Guitarclean,Electric_Guitarmuted,Overdriven_Guitar,Distortion_Guitar,Guitar_Harmonics,
            Acoustic_Bass,Electric_Bassfinger,Electric_Basspick,Fretless_Bass,Slap_Bass_1,Slap_Bass_2,Synth_Bass_1,Synth_Bass_2,
            Violin,Viola,Cello,Contrabass,Tremolo_Strings,Pizzicato_Strings,Orchestral_Strings,Timpani,
            String_Ensemble_1,String_Ensemble_2,SynthStrings_1,SynthStrings_2,Choir_Aahs,Voice_Oohs,Synth_Voice,Orchestra_Hit,
            Trumpet,Trombone,Tuba,Muted_Trumpet,French_Horn,Brass_Section,SynthBrass_1,SynthBrass_2,
            Soprano_Sax,Alto_Sax,Tenor_Sax,Baritone_Sax,Oboe,English_Horn,Bassoon,Clarinet,
            Piccolo,Flute,Recorder,Pan_Flute,Blown_Bottle,Skakuhachi,Whistle,Ocarina,
            Lead_1_square,Lead_2_sawtooth,Lead_3_calliope,Lead_4_chiff,Lead_5_charang,Lead_6_voice,Lead_7_fifths,Lead_8_bass_lead,
            Pad_1_new_age,Pad_2_warm,Pad_3_polysynth,Pad_4_choir,Pad_5_bowed,Pad_6_metallic,Pad_7_halo,Pad_8_sweep,
            FX_1_rain,FX_2_soundtrack,FX_3_crystal,FX_4_atmosphere,FX_5_brightness,FX_6_goblins,FX_7_echoes,FX_8_sci_fi,
            Sitar,Banjo,Shamisen,Koto,Kalimba,Bagpipe,Fiddle,Shanai,
            Tinkle_Bell,Agogo,Steel_Drums,Woodblock,Taiko_Drum,Melodic_Tom,Synth_Drum,Reverse_Cymbal,
            Guitar_Fret_Noise,Breath_Noise,Seashore,Bird_Tweet,Telephone_Ring,Helicopter,Applause,Gunshot_
};

    public class Track
    {
        List<ISequence> sequences;
        public PatchNames Instrument{get; set;}

        public byte Channel { get; set; }

        public int Length { get { return sequences.Count; } }

        public int Duration { get; private set; }

        public IEnumerable<ISequence> Sequences { get { return sequences; } }

        public Track(PatchNames instrument, byte channel)
        {
            this.Instrument = instrument;
            this.Channel = channel;
            sequences = new List<ISequence>();
        }

        public void AddSequence(ISequence seq)
        {
            if (seq == null)
                return;

            this.sequences.Add(seq);
            Duration += seq.Duration;
        }

        public ISequence GetMainSequence()
        {
            return sequences[0];
        }

        public PlaybackInfo GeneratePlaybackInfo(int time = 0)
        {
            PlaybackInfo info = new PlaybackInfo();
            info.Add(time, new PlaybackMessage(Instrument, Channel));
            foreach (var s in sequences)
            {
                info += s.GeneratePlaybackInfo(Channel, time);
                time += (int)(1000 * Note.ToRealDuration(s.Duration));
            }
            return info;
        }

        public override string ToString()
        {
            string str = "" + Instrument.ToString() + " ";
            for(int i = 0; i < sequences.Count; i++)
            {
                str += sequences[i].ToString();

                if (i != sequences.Count - 1)
                    str += " ++ ";
            }
            return str;
        }

    }
}
