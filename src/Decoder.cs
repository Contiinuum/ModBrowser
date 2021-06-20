using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleJSON;
namespace ModBrowser
{
    public class Decoder
    {
        public static void LoadModData()
        {
            List<Mod> mods = new List<Mod>();
            using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, "Mods/Config/modData.json"), FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string data = reader.ReadToEnd();
                    try
                    {
                        var json = JSON.Parse(data);
                        //mods = JsonConvert.DeserializeObject<Mod>(data);
                        for(int i = 0; i < json.Count + 1; i++)
                        {
                            Mod mod = new Mod();
                            mod.repoName = json["Mods"][i]["repoName"];
                            mod.displayRepoName = json["Mods"][i]["displayRepoName"];
                            mod.userName = json["Mods"][i]["userName"];
                            mod.displayUserName = json["Mods"][i]["displayUserName"];
                            mod.description = json["Mods"][i]["description"];
                            mods.Add(mod);
                        }
                        Main.mods = mods;
                    }
                    catch(Exception ex)
                    {
                        MelonLoader.MelonLogger.Warning("Error parsing modData.json: " + ex.Message);
                        Main.mods = null;
                    }                   
                }
            }
        }
    }
}
