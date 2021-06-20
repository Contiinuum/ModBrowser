using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Harmony;

namespace ModBrowser
{
    public class Main : MelonMod
    {
        public static class BuildInfo
        {
            public const string Name = "ModBrowser";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "0.1.0"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }
        
		 public override void OnApplicationStart()
         {
            HarmonyInstance instance = HarmonyInstance.Create("AudicaMod");
            Integrations.LoadIntegrations();
         }

    }
}




















































































