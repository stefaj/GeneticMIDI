using DotNetLearn.Markov;
using GeneticMIDI.Representation;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    [Serializable]
    [ProtoContract]
    public class DrumGenerator : INoteGenerator
    {
          [ProtoMember(1)]
        MarkovChain<Note> chain;

        MelodySequence[] seqs;

        const string SAVE_FILE = "markovdrummer.dat";

        public bool IsInitialized { get; private set; }

        Random random = new Random();

        private string SavePath
        {
            get
            {
                return Constants.LOCAL_SAVE_PATH + System.IO.Path.DirectorySeparatorChar + "Drums" + System.IO.Path.DirectorySeparatorChar + SAVE_FILE;
            }
        }


        public int Order
        {
            get
            {
                return chain.Order;
            }
        }

        public DrumGenerator()
        {
            IsInitialized = false;
            this.chain = new DotNetLearn.Markov.MarkovChain<Note>(4);
        }

        public DrumGenerator(int order)
        {
            this.chain = new DotNetLearn.Markov.MarkovChain<Note>(order);

            IsInitialized = false;
        }

        public void Initialize(Databank bank)
        {
            if (System.IO.File.Exists(SavePath))
            {
                Console.WriteLine("Loading from file");
                LoadFromFile();
            }
            else
            {
                Console.WriteLine("Generating from library");
                Train(bank);
            }
            IsInitialized = true;
        }


        public void Train(Databank bank)
        {
            var cat = bank.Load("Drums");

            int count = 0;
            int percus = 0;            

            foreach (var c in cat.Compositions)
            {
                foreach (var t in c.Tracks)
                {
                    if (t.Channel == 10)
                    {
                        var mel = t.GetMainSequence() as MelodySequence;
                        this.chain.Add(mel.ToArray());
                        percus++;
                    }

                    count++;
                }
            }

            Save();
        }


        private void Save()
        {
            string root = System.IO.Path.GetDirectoryName(SavePath);
            if (!System.IO.Directory.Exists(root))
                System.IO.Directory.CreateDirectory(root);

            System.IO.FileStream fs = new System.IO.FileStream(SavePath, System.IO.FileMode.Create);

            ProtoBuf.Serializer.PrepareSerializer<DrumGenerator>();

            ProtoBuf.Serializer.Serialize(fs, this);

            fs.Close();
        }

        public void LoadFromFile()
        {
            DrumGenerator gen = new DrumGenerator();

            ProtoBuf.Serializer.PrepareSerializer<DrumGenerator>();

            System.IO.FileStream fs = new System.IO.FileStream(SavePath, System.IO.FileMode.Open);

            gen = ProtoBuf.Serializer.Deserialize<DrumGenerator>(fs);

            chain = gen.chain;

            fs.Close();
        }



        public MelodySequence Generate()
        {
            int seed = random.Next();

            var gen_mel = new MelodySequence(chain.Chain(200, seed));

            return gen_mel;
        }

        public MelodySequence Next()
        {
            return Generate();
        }

        public bool HasNext
        {
            get { return true; }
        }

        public PatchNames Instrument
        {
            get { return PatchNames.Synth_Drum; }
        }
    }
}
