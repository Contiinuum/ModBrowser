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

namespace ModBrowser
{
    public static class ModDownloader
    {
        internal static string apiUrl = "https://api.github.com/repos/";
        internal static bool needRefresh = false;
        internal static int page = 1;
        internal static HashSet<string> downloadedFileNames = new HashSet<string>();
        internal static HashSet<string> failedDownloads = new HashSet<string>();
        internal static bool showPopup = false;
        /// <summary>
        /// Coroutine that searches for songs using the web API
        /// </summary>
        /// <param name="search">Query text, e.g. song name, artist or mapper (partial matches possible)</param>
        /// <param name="onSearchComplete">Called with search result once search is completed, use to process search result</param>
        /// <param name="difficulty">Only find songs with given difficulty</param>
        /// <param name="curated">Only find songs that are curated</param>
        /// <param name="sortByPlayCount">Sort result by play count</param>
        /// <param name="page">Page to return (see APISongList.total_pages after initial search (page = 1) to check if multiple pages exist)</param>
        /// <param name="total">Bypasses all query and filter limitations and just returns entire song list</param>
        
        internal static void ShowQueuedPopup()
        {
            showPopup = false;
            Main.TextPopup("Mods updated!");
        }
        public static void GetAllMods()
        {
            foreach(Mod mod in Main.mods)
            {
                string url = $"{apiUrl}{mod.userName}/{mod.repoName}/releases/latest";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                if(request != null)
                {
                    request.Method = "GET";
                    request.UserAgent = "ModBrowser";
                    request.ServicePoint.Expect100Continue = false;
                    try
                    {
                        using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                        {
                            string json = reader.ReadToEnd();
                            var data = SimpleJSON.JSON.Parse(json);
                            mod.version = Version.Parse(data["tag_name"]);
                            mod.downloadLink = data["assets"][0]["browser_download_url"];
                        }
                    }
                    catch(Exception ex)
                    {
                        MelonLogger.Warning("Couldn't get info on " + mod.repoName + ": " + ex.Message);
                        continue;
                    }
                }
            }
            SetModStatus();
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
                    if(Path.GetFileNameWithoutExtension(files[i]) == mod.repoName)
                    {
                        mod.isDownloaded = true;
                        FileVersionInfo modInfo = FileVersionInfo.GetVersionInfo(Path.Combine(modPath, files[i]));
                        Version localVersion = Version.Parse(modInfo.FileVersion);
                        if(localVersion >= mod.version)
                        {
                            mod.isUpdated = true;
                        }
                        break;
                    }
                }
            }

            foreach(Mod mod in Main.mods)
            {
                if(mod.isDownloaded && !mod.isUpdated)
                {
                    MelonCoroutines.Start(DownloadMod(mod, true, true));
                }
            }
        }

        public static async void UpdateModData()
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
                    bytes = await client.DownloadDataTaskAsync(new Uri("https://raw.githubusercontent.com/Contiinuum/ModBrowser/main/ModData.json"));
                    using (FileStream stream = new FileStream(targetPath, FileMode.Create))
                    {
                        await stream.WriteAsync(bytes, 0, bytes.Length);
                    }
                }
                catch(WebException ex)
                {
                    MelonLogger.Warning("Couldn't update modData: " + ex.InnerException.Message);
                }
                Decoder.LoadModData();
                GetAllMods();
            }
        }

        /// <summary>
        /// Coroutine that downloads a song from given download URL. Caller is responsible to call
        /// SongBrowser.ReloadSongList() once download is done
        /// </summary>
        /// <param name="songID">SongID of download target, typically Song.song_id</param>
        /// <param name="downloadUrl">Download target, typically Song.download_url</param>
        /// <param name="onDownloadComplete">Called when download has been written to disk.
        ///     First argument is the songID of the downloaded song.
        ///     Second argument is true if the download succeeded, false otherwise.</param>
        public static IEnumerator DownloadMod(Mod mod, bool update, bool onLoad = false)
        {
            using (WebClient client = new WebClient())
            {
                string targetPath = Path.Combine(Environment.CurrentDirectory, "Mods");
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                targetPath = Path.Combine(targetPath, mod.repoName + ".dll");
                byte[] bytes;
                try
                {
                    client.Headers.Add("user-agent", "ModBrowser");
                    //bytes = await client.DownloadDataTaskAsync(new Uri(mod.downloadLink));
                    bytes = client.DownloadData(new Uri(mod.downloadLink));
                    using (FileStream stream = new FileStream(targetPath, FileMode.Create))
                    {
                        //await stream.WriteAsync(bytes, 0, bytes.Length);
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
                    MelonLogger.Msg("Updated " + mod.repoName);
                    Main.showRestartReminder = true;
                }
                catch (WebException ex)
                {
                    MelonLogger.Warning("Couldn't download " + mod.repoName + ": " + ex.InnerException.Message);
                }
                yield return null;
            }
        }

        public static IEnumerator DeleteMod(Mod mod)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Mods", mod.repoName + ".dll");
            if (File.Exists(path))
            {
                File.Delete(path);
                Main.TextPopup(mod.displayRepoName + " deleted");
                Main.showRestartReminder = true;
            }
            mod.isDownloaded = false;
            mod.isUpdated = false;
            yield return null;    
            
        }
    }
}

