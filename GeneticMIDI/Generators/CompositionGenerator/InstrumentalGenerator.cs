using GeneticMIDI.Representation;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.CompositionGenerator
{
    [Serializable]
    [ProtoContract]
    public class InstrumentalGenerator : IPlaybackGenerator, ICompositionGenerator
    {
        const string SAVE_FILE = "instrumental.dat";

        [ProtoMember(1)]
        Dictionary<PatchNames, Markov.MarkovChain<Note>> instruments = new Dictionary<PatchNames, Markov.MarkovChain<Note>>();

        //[ProtoMember(2)]
        Markov.MarkovChain<ActivityColumn> activityGenerator;

        [ProtoMember(3)]
        Markov.MarkovChain<int> instrumentGenerator;
        
        public event OnFitnessUpdate OnPercentage;

        string path = "";

        private IEnumerable<PatchNames> lastInstruments = null;
        
        public IEnumerable<PatchNames> Instruments
        {
            get
            {
                return instruments.Keys;
            }
        }

        public bool IsInitialized { get; private set; }
        private string SavePath
        {
            get
            {
                return path + System.IO.Path.DirectorySeparatorChar + SAVE_FILE;
            }
        }

        public InstrumentalGenerator(string path)
        {
            activityGenerator = new Markov.MarkovChain<ActivityColumn>(3);
            instrumentGenerator = new Markov.MarkovChain<int>(2);
            this.path = path;            
        }

        public InstrumentalGenerator()
        {
            activityGenerator = new Markov.MarkovChain<ActivityColumn>(3);
            instrumentGenerator = new Markov.MarkovChain<int>(2);
            this.path = ""; 
        }

        public void Initialize()
        {
            if (System.IO.File.Exists(SavePath))
            {
                Console.WriteLine("Loading from file");
                LoadFromFile();
            }
            else
            {
                Console.WriteLine("Generating from library");
                GenerateFromFiles(path);
            }
            IsInitialized = true;
        }


        public void GenerateFromFiles(string path)
        {
            instruments = new Dictionary<PatchNames, Markov.MarkovChain<Note>>();
            Dictionary<PatchNames, List<string>> instrument_tracker = new Dictionary<PatchNames, List<string>>();
            //BinaryFormatter serializer = new BinaryFormatter();
            
            //System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress);
            int i = 0;
            var files = Utils.GetFiles(path);
            int percentage = files.Length / 100;
            //foreach (string f in files)
            Parallel.For(0, files.Length, j =>
            {
                bool continue_ = true;
                var f = files[j];
                if (System.IO.Path.GetExtension(f) != ".mid")
                    continue_ = false;
                var filename = System.IO.Path.GetFileNameWithoutExtension(f);
                Composition comp = new Composition();
                Console.WriteLine("{0:00.00}%\t Adding {1}", (float)i / files.Length * 100.0f, filename);
                try
                {
                    comp = Composition.LoadFromMIDI(f);
                }
                catch
                {
                    Console.WriteLine("Skipping {0}", filename);
                    continue_ = false;
                }
                List<int> instruments_ints = new List<int>();
                if(continue_)
                foreach (var track in comp.Tracks)
                {
                    instruments_ints.Add((int)track.Instrument);
                    var mel = track.GetMainSequence() as MelodySequence;
                    if (!instruments.ContainsKey(track.Instrument))
                    {
                        instruments[track.Instrument] = new Markov.MarkovChain<Note>(3);
                        instrument_tracker[track.Instrument] = new List<string>();
                    }
                    lock (instrument_tracker[track.Instrument])
                    {
                        instrument_tracker[track.Instrument].Add(filename);
                        instruments[track.Instrument].Add(mel.ToArray());
                    }
                }
                instrumentGenerator.Add(instruments_ints);

                //Report progress
                if (i > percentage)
                {
                    if (OnPercentage != null)
                        OnPercentage(this, i, 0);
                    percentage += files.Length / 100;
                }

                ActivityMatrix matrix = ActivityMatrix.GenerateFromComposition(comp);
                activityGenerator.Add(matrix.ActivityColumns);

                i++;
            });

            foreach (var k in instrument_tracker.Keys)
            {
                Console.WriteLine("Instrument {0} with count {1}", k, instrument_tracker[k].Count);
            }

            Save();

            Console.WriteLine("Done");
        }

        private void Save()
        {
            System.IO.FileStream fs = new System.IO.FileStream(SavePath, System.IO.FileMode.Create);

            ProtoBuf.Serializer.PrepareSerializer<InstrumentalGenerator>();

            ProtoBuf.Serializer.Serialize(fs, this);

            fs.Close();
        }

        public void LoadFromFile()
        {
            InstrumentalGenerator gen = new InstrumentalGenerator(path);

            ProtoBuf.Serializer.PrepareSerializer<InstrumentalGenerator>();

            System.IO.FileStream fs = new System.IO.FileStream(SavePath, System.IO.FileMode.Open);

            gen = ProtoBuf.Serializer.Deserialize<InstrumentalGenerator>(fs);

            instruments = gen.instruments;
            instrumentGenerator = gen.instrumentGenerator;
            activityGenerator = gen.activityGenerator;

            fs.Close();
        }

        MelodySequence GenerateInstrument(PatchNames patch, int seed)
        {
            var chain = instruments[patch];
            return new MelodySequence(chain.Chain(seed));
        }

        public Composition Generate(IEnumerable<PatchNames> instrs, int seed = 0)
        {
            lastInstruments = instrs;
            int i = 1;
            Composition comp = new Composition();
            foreach(PatchNames instrument in instrs)
            {
                if(instruments.ContainsKey(instrument))
                {
                    Track t = new Track(instrument, (byte)(i++));
                    t.AddSequence(GenerateInstrument(instrument, seed));
                    comp.Add(t);
                }
            }
            return comp;
        }

        public Composition Generate(int seed = 0)
        {
            lastInstruments = null;
            Composition comp = new Composition();
            /*Random ra = new Random(seed);
            for(int i = 0; i < 4; i++)
            {
                var keys = instruments.Keys.ToArray();
                PatchNames instrument = keys[ra.Next(0, instruments.Keys.Count)];

                Track track = new Track(instrument, (byte)(i + 1));
                track.AddSequence(GenerateInstrument(instrument, seed));
                comp.Add(track);
            }*/

            Random rand = new Random(seed);
            var keys = instruments.Keys.ToArray();
            PatchNames instrument = keys[rand.Next(0, instruments.Keys.Count)];

            var instruments_ = instrumentGenerator.Chain(new int[]{(int)instrument}, seed);
            Track track = new Track(instrument, 1);
            track.AddSequence(GenerateInstrument(instrument, seed++));
            comp.Add(track);

            int j = 2;
            foreach(var i in instruments_)
            {
                var instr = (PatchNames)i;
                track = new Track(instr, (byte)(j));
                track.AddSequence(GenerateInstrument(instr, seed++));
                comp.Add(track);
                if (j > 3)
                    break;
                j++;
            }

            return comp;
        }

        public Representation.PlaybackInfo GeneratePlayback()
        {
            return Generate().GeneratePlaybackInfo();
        }

        public Composition GenerateComposition()
        {
            return Generate();
        }
        

        IPlayable IPlaybackGenerator.Generate()
        {
            return Generate();
        }

        int last_next = 0;
        IPlayable IPlaybackGenerator.Next()
        {
            if (lastInstruments == null)
                return Generate(last_next++);
            else
                return Generate(lastInstruments, last_next++);
        }

        bool IPlaybackGenerator.HasNext
        {
            get { return true; }
        }
    }
}
