using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using System.Collections;

namespace ModBrowser
{
    public class Main : MelonMod
    {
        public static List<Mod> mods = new List<Mod>();
        public static bool showRestartReminder = false;
        public static List<Mod> pendingDelete = new List<Mod>();
        public static List<Mod> pendingUpdate = new List<Mod>();
        public static class BuildInfo
        {
            public const string Name = "ModBrowser";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "1.0.1"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }


        public override void OnApplicationStart()
        {
            Config.RegisterConfig();
            ModDownloader.GetRateLimit();
            bool shouldUpdate = ModDownloader.CheckUpdate();
            MelonCoroutines.Start(ModDownloader.UpdateModData(shouldUpdate));
        }       
        

        public override void OnApplicationQuit()
        {
            ModDownloader.HandlePendingUpdates();
            ModDownloader.DeleteMods();
            Decoder.SaveModsToCache();
        }

        public static void TextPopup(string text)
        {
            KataConfig.I.CreateDebugText(text, new Vector3(0f, -1f, 5f), 5f, null, false, 0.2f);
        }     
    }
}




















































































