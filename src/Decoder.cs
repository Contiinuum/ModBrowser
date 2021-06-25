using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleJSON;
using MelonLoader;
namespace ModBrowser
{
    public class Decoder
    {
        public static void LoadModData()
        {
            LoadCachedMods();
            List<Mod> mods = new List<Mod>();
            using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, "Mods/Config/modData.json"), FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string data = reader.ReadToEnd();
                    try
                    {
                        var json = JSON.Parse(data);
                        for(int i = 0; i < json["Mods"].Count; i++)
                        {
                            Mod mod = new Mod();
                            mod.repoName = json["Mods"][i]["repoName"];
                            mod.displayRepoName = json["Mods"][i]["displayRepoName"];
                            mod.userName = json["Mods"][i]["userName"];
                            mod.displayUserName = json["Mods"][i]["displayUserName"];
                            mod.description = json["Mods"][i]["description"];
                            if(!Main.mods.Any(m => m.repoName == mod.repoName))
                            {
                                mods.Add(mod);
                            }
                        }

                        //Main.mods = mods;
                        Main.mods.AddRange(mods);
                        Main.mods.Sort((x, y) => string.Compare(x.displayRepoName, y.displayRepoName));
                    }
                    catch(Exception ex)
                    {
                        MelonLogger.Warning("Error parsing modData.json: " + ex.Message);
                        Main.mods = null;
                    }                   
                }
            }
            CheckDeletedMods();
        }

        private static void CheckDeletedMods()
        {
            var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Mods"), "*.dll");
            foreach(Mod mod in Main.mods)
            {
                if (!files.Any(f => Path.GetFileName(f) == mod.fileName))
                {
                    if (mod.isDownloaded)
                    {
                        mod.isDownloaded = false;
                        mod.isUpdated = false;
                    }
                }
            }
        }

        private static void LoadCachedMods()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Mods/Config/modCache.json");
            if (!File.Exists(path)) return;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string data = reader.ReadToEnd();
                    try
                    {
                        List<Mod> mods = JsonConvert.DeserializeObject<List<Mod>>(data);
                        if (mods != null && mods.Count > 0) Main.mods = mods;
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning("Error parsing modCache.json: " + ex.Message);
                    }
                }
            }
        }

        public static void SaveModsToCache()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Mods/Config/modCache.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(Main.mods, Formatting.Indented));
        }
    }
}
