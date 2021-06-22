using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Harmony;
using System.Collections;
using System.Linq;

namespace ModBrowser
{
    public class Main : MelonMod
    {
        public static List<Mod> mods = new List<Mod>();
        public static bool showRestartReminder = false;
        public static List<Mod> pendingDelete = new List<Mod>();
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
            Config.RegisterConfig();
            Integrations.LoadIntegrations();
            //TimeSpan limit = TimeSpan.FromMinutes(60);
            //DateTime lastUpdate = DateTime.Parse(Config.lastUpdateCheck);
            //bool shouldUpdate = true;
            //MelonLogger.Warning(DateTime.UtcNow - lastUpdate);
            Config.UpdateValue(nameof(Config.lastUpdateCheck), DateTime.UtcNow.ToString());
            MelonLogger.Msg("Checking for updates..");
            /*if (DateTime.UtcNow - lastUpdate > limit)
            {
            }
            else
            {
                shouldUpdate = false;
                MelonLogger.Msg("Checked for updates less than an hour ago. Skipping.");
            }*/
            ModDownloader.GetRateLimit();
            bool shouldUpdate = ModDownloader.CheckUpdate();
            MelonCoroutines.Start(ModDownloader.UpdateModData(shouldUpdate));
        }

        public override void OnApplicationQuit()
        {
            ModDownloader.DeleteMods();
            Decoder.SaveModsToCache();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                InGameUI.I.GoToPausePage();
            }
        }

        public static void TextPopup(string text)
        {
            KataConfig.I.CreateDebugText(text, new Vector3(0f, -1f, 5f), 5f, null, false, 0.2f);
        }

        public static void ShowRestartReminder()
        {
            showRestartReminder = false;
            MelonCoroutines.Start(IShowRestartReminder());
        }

        private static IEnumerator IShowRestartReminder()
        {
            Decoder.SaveModsToCache();
            yield return new WaitForSecondsRealtime(.5f);
            TextPopup("Please restart your game!");
        }
    }
}




















































































