using System;
using System.Collections;
using System.Net;
using MelonLoader;
using UnityEngine;
using MelonLoader.TinyJSON;
using System.IO;
using System.Media;
using System.Collections.Generic;
using System.Reflection;
using SimpleJSON;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;

namespace ModBrowser
{
    public static class ModDownloader
    {
        internal static string apiUrl = "https://api.github.com/repos/";
        internal static string rateLimitUrl = "https://api.github.com/rate_limit";
        internal static bool needRefresh = false;
        internal static bool showPopup = false;
        internal static int remainingRequests = 0;
        private static string modDataETag;

        internal static void ShowQueuedPopup()
        {
            showPopup = false;
            Main.TextPopup("Mods updated - please restart.");
        }

        internal static void GetRateLimit()
        {           
            HttpWebRequest request = WebRequest.Create(rateLimitUrl) as HttpWebRequest;
            if (request != null)
            {
                request.Method = "GET";
                request.UserAgent = "ModBrowser";
                request.ServicePoint.Expect100Continue = false;
                try
                {
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string json = reader.ReadToEnd();
                        var data = SimpleJSON.JSON.Parse(json);
                        remainingRequests = data["rate"]["remaining"];
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse httpResponse = ex.Response as HttpWebResponse;
                    if (httpResponse.StatusCode != HttpStatusCode.NotModified)
                    {
                        MelonLogger.Warning("Couldn't get rate limit: " + httpResponse.StatusDescription);
                    }
                }
            }
        }
        public static IEnumerator GetAllModInfos()
        {
            foreach (Mod mod in Main.mods)
            {
                GetModInfo(mod);
                yield return null;
            }
            SetModStatus();
        }

        public static void UseRequest(string usedFor)
        {
            remainingRequests--;
            MelonLogger.Msg(usedFor);
        }

        public static void GetModInfo(Mod mod)
        {
            List<MelonMod> mods = MelonHandler.Mods;
            MelonMod melon = mods.FirstOrDefault(m => m.Assembly.GetName().ToString() == mod.repoName);
            string url = $"{apiUrl}{mod.userName}/{mod.repoName}/releases/latest";
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Headers.Add("If-None-Match", mod.eTag);
            if (request != null)
            {
                request.Method = "GET";
                request.UserAgent = "ModBrowser";
                request.ServicePoint.Expect100Continue = false;
                try
                {
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        return;
                    }
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        UseRequest("Get Info for " + mod.displayRepoName);
                        mod.eTag = response.Headers["ETag"];
                        string json = reader.ReadToEnd();
                        var data = SimpleJSON.JSON.Parse(json);
                        mod.version = data["tag_name"];
                        mod.downloadLink = data["assets"][0]["browser_download_url"];
                        mod.fileName = data["assets"][0]["name"];
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse httpResponse = ex.Response as HttpWebResponse;
                    if (httpResponse.StatusCode != HttpStatusCode.NotModified)
                    {
                        MelonLogger.Msg("Couldn't get info on " + mod.displayRepoName + ": " + httpResponse.StatusDescription);
                    }
                }
            }
        }

        public static void SetModStatus()
        {
            string modPath = Path.Combine(Environment.CurrentDirectory, "Mods");
            string[] files = Directory.GetFiles(modPath);
            bool noMods = files.Length == 0;
            foreach (Mod mod in Main.mods)
            {
                if (noMods)
                {
                    mod.isDownloaded = false;
                    mod.isUpdated = false;
                    continue;
                }
                for (int i = 0; i < files.Length; i++)
                {
                    string nameToCheck = mod.fileName is null || mod.fileName == "" ? mod.repoName + ".dll" : mod.fileName;
                    if(Path.GetFileName(files[i]) == nameToCheck)
                    {
                        mod.isDownloaded = true;
                        FileVersionInfo modInfo = FileVersionInfo.GetVersionInfo(Path.Combine(modPath, files[i]));
                        Version localVersion = Version.Parse(modInfo.FileVersion);
                        if(mod.version is null || mod.version == "")
                        {
                            mod.isUpdated = false;
                        }
                        else
                        {
                            Version apiVersion = Version.Parse(mod.version);
                            if (localVersion.CompareTo(apiVersion) >= 0) mod.isUpdated = true;
                        }
                        break;
                    }
                }
            }
            Decoder.SaveModsToCache();
            foreach(Mod mod in Main.mods)
            {
                if(mod.isDownloaded && !mod.isUpdated)
                {
                    MelonCoroutines.Start(DownloadMod(mod, true, true));
                }
            }
        }

        public static bool CheckUpdate()
        {
            if(remainingRequests == 0)
            {
                MelonLogger.Msg("All requests used.");
                return false;
            }
            HttpWebRequest request = WebRequest.Create("https://raw.githubusercontent.com/Contiinuum/ModBrowser/main/ModData.json") as HttpWebRequest;
            if (request != null)
            {
                request.Method = "GET";
                request.UserAgent = "ModBrowser";
                request.ServicePoint.Expect100Continue = false;
                request.Headers.Add("If-None-Match", Config.modDataETag);
                try
                {
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        return false;
                    }
                    else
                    {
                        modDataETag = response.Headers["ETag"];
                        UseRequest("Update mod data..");
                        return true;
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse httpResponse = ex.Response as HttpWebResponse;
                    if(httpResponse.StatusCode != HttpStatusCode.NotModified)
                    {
                        MelonLogger.Warning("Couldn't get info on mod data: " + httpResponse.StatusDescription);
                    }
                    return false;
                }
            }
            return false;
        }

        public static IEnumerator UpdateModData(bool shouldUpdate)
        {
            if (shouldUpdate && remainingRequests > 0)
            {
                using (WebClient client = new WebClient())
                {
                    string targetPath = Path.Combine(Environment.CurrentDirectory, "Mods/Config");
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }
                    targetPath = Path.Combine(targetPath, "modData.json");
                    byte[] bytes;
                    try
                    {
                        client.Headers.Add("user-agent", "ModBrowser");
                        bytes = client.DownloadData(new Uri("https://raw.githubusercontent.com/Contiinuum/ModBrowser/main/ModData.json"));
                        UseRequest("Mod data updated.");
                        using (FileStream stream = new FileStream(targetPath, FileMode.Create))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                            Config.UpdateValue(nameof(Config.modDataETag), modDataETag);
                        }
                    }
                    catch (WebException ex)
                    {
                        MelonLogger.Warning("Couldn't update modData: " + ex.InnerException.Message);
                    }
                }
            }           
            yield return null;
            Decoder.LoadModData();
            MelonCoroutines.Start(GetAllModInfos());
        }

        public static IEnumerator DownloadMod(Mod mod, bool update, bool onLoad = false)
        {
            if(Main.pendingDelete.FirstOrDefault(m => m.displayRepoName == mod.displayRepoName) is Mod pending)
            {
                if(pending != null)
                {
                    MelonLogger.Msg("Pending found");
                    Main.pendingDelete.Remove(pending);
                    LoadMod(mod, true);
                    yield break;
                }
               
            }
            if(remainingRequests == 0)
            {
                MelonLogger.Msg("All requests used.");
                yield break;
            }
            using (WebClient client = new WebClient())
            {
                string targetPath = onLoad ? Path.Combine(Environment.CurrentDirectory, "Downloads") : Path.Combine(Environment.CurrentDirectory, "Mods"); //"Mods"
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                targetPath = Path.Combine(targetPath, mod.fileName);
                byte[] bytes;
                try
                {
                    client.Headers.Add("user-agent", "ModBrowser");
                    
                    bytes = client.DownloadData(new Uri(mod.downloadLink));
                    UseRequest("Downloading " + mod.displayRepoName + "..");
                    MelonMod melon = MelonHandler.Mods.FirstOrDefault(m => m.Assembly.GetName().Name + ".dll" == mod.fileName);
                    if (melon != null) melon.HarmonyInstance.UnpatchSelf();
                    using (FileStream stream = new FileStream(targetPath, FileMode.Create))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        mod.isDownloaded = true;
                        mod.isUpdated = true;
                        if (!onLoad)
                        {
                            string txt = update ? " updated" : " downloaded";
                            Main.TextPopup(mod.displayRepoName + txt);
                        }
                        else
                        {
                            showPopup = true;
                        }
                    }
                    
                    Main.showRestartReminder = true;
                }
                catch (WebException ex)
                {
                    MelonLogger.Warning("Couldn't download " + mod.repoName + ": " + ex.InnerException.Message);
                }
                yield return null;
            }
            //yield return new WaitForSecondsRealtime(2f);
            if (onLoad) Main.pendingUpdate.Add(mod);
            else LoadMod(mod, false);



            GetModInfo(mod);
        }

        private static void LoadMod(Mod mod, bool reenable)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Mods", mod.fileName);
            if(!reenable) MelonHandler.LoadFromFile(path);
            List<MelonMod> mods = MelonHandler.Mods;
            MelonMod melon = mods.FirstOrDefault(m => m.Assembly.GetName().Name + ".dll" == mod.fileName);
            if (melon != null)
            {
                MelonLogger.Msg(mod.displayRepoName + " downloaded.");
                mod.isDownloaded = true;
                mod.isUpdated = true;
                if (mod.repoName == "ModBrowser") return;
                if(!reenable)
                {                  
                    melon.OnApplicationStart();
                }
                melon.HarmonyInstance.PatchAll(melon.Assembly);
            }
        }

        public static void HandlePendingUpdates()
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "Downloads");
            string destPath = Path.Combine(Environment.CurrentDirectory, "Mods");
            if (Directory.Exists(sourcePath))
            {
                foreach (Mod mod in Main.pendingUpdate)
                {
                    string sourceFile = Path.Combine(sourcePath, mod.fileName);
                    string destFile = Path.Combine(destPath, mod.fileName);
                    if (File.Exists(sourceFile))
                    {
                        if (File.Exists(destFile)) File.Delete(destFile);
                        File.Move(sourceFile, destFile);
                        Mod mainMod = Main.mods.FirstOrDefault(m => m.repoName == mod.repoName);
                        if(mainMod != null)
                        {
                            mainMod.isDownloaded = true;
                            mainMod.isUpdated = true;
                        }
                    }
                }
            }
        }

        public static void DisableMod(Mod mod)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Mods", mod.fileName);
            List<MelonMod> mods = MelonHandler.Mods;
            MelonMod melon = mods.FirstOrDefault(m => m.Assembly.GetName().Name + ".dll" == mod.fileName);
            if(melon != null)
            {
                MelonLogger.Msg("Unpatching " + mod.displayRepoName + "..");
                melon.HarmonyInstance.UnpatchSelf();
                mod.isDownloaded = false;
                mod.isUpdated = false;
                Main.pendingDelete.Add(mod);
            }
            Main.TextPopup(mod.displayRepoName + " deleted");
            
        }

        public static void DeleteMods()
        {
            if (Main.pendingDelete is null || Main.pendingDelete.Count == 0) return;
            foreach(Mod mod in Main.pendingDelete)
            {
                string path = Path.Combine(Environment.CurrentDirectory, "Mods", mod.fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                mod.isDownloaded = false;
                mod.isUpdated = false;
            }
        }
    }
}

