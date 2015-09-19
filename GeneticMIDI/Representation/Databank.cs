using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{


    public class Databank
    {
        Dictionary<string, CompositionCategory> categories;

        public static Dictionary<string, string> defaultPaths;

        string path;

        public static void GenerateDefaultPaths(string dir)
        {
            defaultPaths = new Dictionary<string, string>();
            defaultPaths["Classical"] = dir + @"\Classical\Mixed";
            defaultPaths["Classic Rock"] = dir + @"\ClassicRock";
            defaultPaths["Dance Techno"] = dir + @"\Dance_Techno";
            defaultPaths["Jazz"] = dir + @"\Jazz";
            defaultPaths["Pop"] = dir + @"\Pop_and_Top40";
            defaultPaths["Video Games"] = dir + @"\Video_Games";
            defaultPaths["Classical Piano"] = dir + @"\Classical Piano Midis";
            defaultPaths["Test"] = dir + @"\Test";
        }

        public Databank(string dir)
        {
            categories = new Dictionary<string, CompositionCategory>();
            path = dir;
            GenerateDefaultPaths(dir);
        }

        public Databank()
        {
            categories = new Dictionary<string, CompositionCategory>();
        }

        /// <summary>
        /// Lazy loader
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public CompositionCategory Load(string categoryName)
        {
            if(categories.ContainsKey(categoryName))
            {
                if (categories[categoryName].Compositions != null && categories[categoryName].Compositions.Length > 0)
                    return categories[categoryName];

            }
            else if (defaultPaths.ContainsKey(categoryName))
            {
                  categories[categoryName] = CompositionCategory.LoadFromFile(defaultPaths[categoryName], categoryName);
                  return categories[categoryName];
            }
            throw new Exception("Not found");
        }

        public void Unload(string categoryName)
        {
            if(categories.ContainsKey(categoryName))
            {
                categories[categoryName].Clear();
                categories.Remove(categoryName);
                GC.Collect();
            }
        }

        public void UnloadAll()
        {
            foreach (var key in categories.Keys.ToArray())
                Unload(key);
        }


        /// <summary>
        /// Loads compositions from directories and saves as file
        /// </summary>
        /// <param name="libraryDir"></param>
        /// <param name="saveDir"></param>
        /// <returns></returns>
        public static Databank GenerateCategories(string libraryDir, string saveDir)
        {
            Databank db = new Databank();

            db.categories = new Dictionary<string, CompositionCategory>();

            Console.WriteLine("Generating categories");

            GenerateDefaultPaths("");
            foreach (var key in defaultPaths.Keys)
            {
                CompositionCategory cat = new CompositionCategory(key, libraryDir + defaultPaths[key]);
                Console.WriteLine("Generating compositions for category {0}", key);
                cat.LoadCompositions(true);
                db.categories[key] = cat;

                Console.WriteLine("Saving compositions for category {0}", key);
                db.categories[key].Save(saveDir + defaultPaths[key]);

                db.Unload(key); // Free from memory
            }

            GenerateDefaultPaths(libraryDir);

            return db;
        }

        public static Databank PreloadAll(string dir)
        {
            Databank db = new Databank(dir);
            db.LoadAll();
            return db;
      
        }

        public void LoadAll()
        {
            UnloadAll();
            categories = new Dictionary<string, CompositionCategory>();


            GenerateDefaultPaths(path);
            foreach (var key in defaultPaths.Keys)
            {
                Load(key);
            }
        }
    }
}
