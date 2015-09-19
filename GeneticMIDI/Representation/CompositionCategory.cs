using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    [Serializable]
    [ProtoContract]
    public class CompositionCategory
    {
        [ProtoMember(1)]
        public string CategoryName { get; private set; }

        [ProtoMember(2)]
        public string OriginalPath { get; private set; }

        [ProtoMember(3)]
        public Composition[] Compositions { get; private set; }

        public bool IsLoaded
        {
            get
            {
                if (Compositions == null)
                    return false;
                if (Compositions.Length < 1)
                    return false;
                return true;
            }
        }

        public CompositionCategory()
        {
            Compositions = null;
        }

        public CompositionCategory(string name, string path)
        {
            this.OriginalPath = path;
            this.CategoryName = name;
        }

        public void Clear()
        {
            this.CategoryName = "";
            this.OriginalPath = "";
            this.Compositions = null;
        }

        public void LoadCompositions(bool parallel=false)
        {
            if(!parallel)
                this.Compositions = Utils.LoadCompositionsFromDirectory(this.OriginalPath);
            else
                this.Compositions = Utils.LoadCompositionsParallel(this.OriginalPath);
        }

        public static CompositionCategory LoadFromFile(string path, string category)
        {
            return LoadFromFile(GetFilePath(path, category));
        }

        public static CompositionCategory LoadFromFile(string fullPath)
        {
            CompositionCategory cat = new CompositionCategory();

            ProtoBuf.Serializer.PrepareSerializer<CompositionCategory>();

            System.IO.FileStream fs = new System.IO.FileStream(fullPath, System.IO.FileMode.Open);
            System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress);


           // System.IO.MemoryStream memStream = new System.IO.MemoryStream();
           // gz.CopyTo(memStream);

            cat = ProtoBuf.Serializer.Deserialize<CompositionCategory>(gz);

            fs.Close();

            return cat;
        }


        public static string GetFilePath(string dir, string category)
        {
            return dir + "/cat_" + category.ToLower() + ".dat";
        }

        public Composition GetRandomComposition()
        {
            Random rnd = new Random();
            int r = rnd.Next(0, Compositions.Length);
            return Compositions[r];
        }

        public void Save(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            System.IO.FileStream fs = new System.IO.FileStream(GetFilePath(path, this.CategoryName), System.IO.FileMode.Create);
            System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress);

            ProtoBuf.Serializer.PrepareSerializer<CompositionCategory>();

            ProtoBuf.Serializer.Serialize(gz, this);

            gz.Flush();

            gz.Close();

            fs.Close();
        }

    }

}
