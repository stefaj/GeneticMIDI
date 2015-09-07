using GeneticMIDI.Representation;
using MachineLearningTestEnvironment;
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
    public class InstrumentalGenerator2 : IPlaybackGenerator
    {
        const string SAVE_FILE = "instrumental2.dat";

        [ProtoMember(1)]
        Dictionary<PatchNames, MarkovChain<Note>> instruments = new Dictionary<PatchNames, MarkovChain<Note>>();

        int last_next = 0;

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

        public InstrumentalGenerator2(string path)
        {
            this.path = path;
        }

        public InstrumentalGenerator2()
        {
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
            instruments = new Dictionary<PatchNames, MarkovChain<Note>>();
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
                if (continue_)
                    foreach (var track in comp.Tracks)
                    {
                        instruments_ints.Add((int)track.Instrument);
                        var mel = track.GetMainSequence() as MelodySequence;
                        if (!instruments.ContainsKey(track.Instrument))
                        {
                            instruments[track.Instrument] = new MarkovChain<Note>(3);
                            instrument_tracker[track.Instrument] = new List<string>();
                        }
                        lock (instrument_tracker[track.Instrument])
                        {
                            instrument_tracker[track.Instrument].Add(filename);
                            instruments[track.Instrument].Add(mel.ToArray());
                        }
                    }


                //Report progress
                if (i > percentage)
                {
                    if (OnPercentage != null)
                        OnPercentage(this, i, 0);
                    percentage += files.Length / 100;
                }

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

            ProtoBuf.Serializer.PrepareSerializer<InstrumentalGenerator2>();

            ProtoBuf.Serializer.Serialize(fs, this);

            fs.Close();
        }

        public void LoadFromFile()
        {
            InstrumentalGenerator2 gen = new InstrumentalGenerator2(path);

            ProtoBuf.Serializer.PrepareSerializer<InstrumentalGenerator2>();

            System.IO.FileStream fs = new System.IO.FileStream(SavePath, System.IO.FileMode.Open);

            gen = ProtoBuf.Serializer.Deserialize<InstrumentalGenerator2>(fs);

            instruments = gen.instruments;

            fs.Close();
        }

        MelodySequence GenerateInstrument(PatchNames patch, int seed)
        {
            var chain = instruments[patch];
            return new MelodySequence(chain.Chain(100,seed));
        }

        public Composition Generate(IEnumerable<PatchNames> instrs, int seed = 0)
        {
            lastInstruments = instrs;
            int i = 1;
            Composition comp = new Composition();
            foreach (PatchNames instrument in instrs)
            {
                if (instruments.ContainsKey(instrument))
                {
                    Track t = new Track(instrument, (byte)(i++));
                    t.AddSequence(GenerateInstrument(instrument, seed));
                    comp.Add(t);
                }
            }
            return comp;
        }



        public IPlayable Generate()
        {
            return Generate();
        }

        public IPlayable Next()
        {
            return Generate(lastInstruments, last_next++);
        }

        public bool HasNext
        {
            get { return true; }
        }

        public Composition GenerateComposition()
        {
            return Generate(lastInstruments);
        }
    }
}
