using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.CompositionGenerator
{

    class InstrumentalGenerator : IPlaybackGenerator, ICompositionGenerator
    {
        const string SAVE_FILE = "instrumental.dat";
        Dictionary<PatchNames, Markov.MarkovChain<Note>> instruments = new Dictionary<PatchNames, Markov.MarkovChain<Note>>();

        Markov.MarkovChain<ActivityColumn> activityGenerator;
        public InstrumentalGenerator(string path)
        {
            activityGenerator = new Markov.MarkovChain<ActivityColumn>(3);
            if (System.IO.File.Exists(SAVE_FILE))
            {
                Console.WriteLine("Loading from file");
                LoadFromFile();
            }
            else
            {
                Console.WriteLine("Generating from library");
                GenerateFromFiles(path);
            }
            
        }
        public void GenerateFromFiles(string path)
        {
            instruments = new Dictionary<PatchNames, Markov.MarkovChain<Note>>();
            Dictionary<PatchNames, List<string>> instrument_tracker = new Dictionary<PatchNames, List<string>>();
            //BinaryFormatter serializer = new BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(SAVE_FILE, System.IO.FileMode.Create);
            ProtoBuf.Serializer.PrepareSerializer<Dictionary<PatchNames, Markov.MarkovChain<Note>>>();
            //System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress);
            int i = 0;
            var files = System.IO.Directory.GetFiles(path);
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
                if(continue_)
                foreach (var track in comp.Tracks)
                {
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

                ActivityMatrix matrix = ActivityMatrix.GenerateFromComposition(comp);
                activityGenerator.Add(matrix.ActivityColumns);

                i++;
            });

            foreach (var k in instrument_tracker.Keys)
            {
                Console.WriteLine("Instrument {0} with count {1}", k, instrument_tracker[k].Count);
            }

            // try saving
           // serializer.Serialize(fs, instruments);
            ProtoBuf.Serializer.Serialize<Dictionary<PatchNames, Markov.MarkovChain<Note>>>(fs, instruments);
            //gz.Flush();
            //gz.Close();
            fs.Close();

            Console.WriteLine("Done");
        }

        public void LoadFromFile()
        {
           // BinaryFormatter serializer = new BinaryFormatter();
            ProtoBuf.Serializer.PrepareSerializer<Dictionary<PatchNames, Markov.MarkovChain<Note>>>();

            System.IO.FileStream fs = new System.IO.FileStream(SAVE_FILE, System.IO.FileMode.Open);
            //System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress);
            //instruments = serializer.UnsafeDeserialize(fs, null) as Dictionary<PatchNames, Markov.MarkovChain<Note>>;
            instruments = ProtoBuf.Serializer.Deserialize<Dictionary<PatchNames, Markov.MarkovChain<Note>>>(fs);
            //gz.Close();
            fs.Close();
        }

        MelodySequence GenerateInstrument(PatchNames patch, int seed)
        {
            var chain = instruments[patch];
            return new MelodySequence(chain.Chain(seed));
        }

        public Composition Generate(IEnumerable<PatchNames> instrs, int seed = 0)
        {
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
            Composition comp = new Composition();
            Random ra = new Random(seed);
            for(int i = 0; i < 4; i++)
            {
                var keys = instruments.Keys.ToArray();
                PatchNames instrument = keys[ra.Next(0, instruments.Keys.Count)];

                Track track = new Track(instrument, (byte)(i + 1));
                track.AddSequence(GenerateInstrument(instrument, seed));
                comp.Add(track);
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
    }
}
