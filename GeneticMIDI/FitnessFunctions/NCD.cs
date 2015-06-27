using AForge.Genetic;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.FitnessFunctions
{
    public class NCD : IFitnessFunction
    {
        string[] songs;

        
        public NCD(IEnumerable<MelodySequence> seqs)
        {
            string notes = "";
            foreach (var m in seqs)
                notes += m.GetNoteStr();
            string hash = MD5Hash(notes);

            string savepath =  "ncd_" + hash + ".dat";
            if(File.Exists(savepath))
            {
                Deserialize(savepath);
                return;
            }


            songs = new string[seqs.Count()];
            
            int i = 0;
            int j = 0;
            foreach (MelodySequence m in seqs)
            {
                Note[] song;

                song = m.ToArray();

                songs[i++] = m.GetNoteStr();
            }

            Serialize(savepath);
        }

        public float ComputeFitness(IEnumerable<Note> individual)
        {
            string indi2str = MelodySequence.GetNoteStr(individual);
            float sum = 0;
            foreach(string str in songs)
            {
                if (str == null)
                    continue;
                float ncd = ComputeNCD(str, indi2str);
                sum += ncd;
            }
            return 1 / sum;

        }

        static float ComputeNCD(string indi1str, string indi2str)
        {
            byte[] zipped1 = Zip(indi1str);
            byte[] zipped2 = Zip(indi2str);
            byte[] zipped3 = Zip(indi1str + indi2str);

            int Z_min = Math.Min(zipped1.Length, zipped2.Length);
            int Z_max = Math.Max(zipped1.Length, zipped2.Length);

            int Zxy = zipped3.Length;

            float ncd = (float)(Zxy - Z_min) / (float)Z_max;
            return ncd;
        }

        static float ComputeNCD(string indi1str, IEnumerable<Note> indi2)
        {
            string indi2str = MelodySequence.GetNoteStr(indi2);
            return ComputeNCD(indi1str, indi2str);
        }

        static float ComputeNCD(IEnumerable<Note> indi1, IEnumerable<Note> indi2)
        {
            string indi1str = MelodySequence.GetNoteStr(indi1);
            return ComputeNCD(indi1str, indi2);

        }

        static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            { 
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }


        public double Evaluate(IChromosome chromosome)
        {
            var chromo = chromosome as GPCustomTree;
            if (chromo == null)
                return 0;
            var notes = chromo.GenerateNotes();
            return ComputeFitness(notes);
        }

        public void Serialize(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, songs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public void Deserialize(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                string[] songs = (string[])formatter.Deserialize(fs);
                
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

        }

        private string MD5Hash(string str)
        {
            byte[] encodedPassword = new UTF8Encoding().GetBytes(str);

            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

            // string representation (similar to UNIX format)
            string encoded = BitConverter.ToString(hash)
                // without dashes
               .Replace("-", string.Empty)
                // make lowercase
               .ToLower();
            return encoded;

        }
    }
}
