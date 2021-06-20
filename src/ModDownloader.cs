using System;
using System.Collections;
using System.Net;
using MelonLoader;
using UnityEngine;
using MelonLoader.TinyJSON;
using System.IO;
using System.Media;
using System.Collections.Generic;

namespace ModBrowser
{
    public static class ModDownloader
    {
        internal static string apiUrl = "https://www.github.com/";
        internal static APIModList modList;
        internal static bool needRefresh = false;
        internal static int page = 1;
        internal static HashSet<string> downloadedFileNames = new HashSet<string>();
        internal static HashSet<string> failedDownloads = new HashSet<string>();

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
        public static IEnumerator DoSongWebSearch(Action<string, APIModList> onSearchComplete)
        {
            string concatURL = "http://www.audica.wiki:5000/api/customsongs?pagesize=all";
            WWW www = new WWW(concatURL);
            yield return www;
            onSearchComplete(search, JSON.Load(www.text).Make<APIModList>());
        }

        public static IEnumerator UpdateModData()
        {
            string modDataURL = "https://www.github.com/Contiinuum/ModBrowser/src"
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
        public static IEnumerator DownloadSong(string songID, string downloadUrl, Action<string, bool> onDownloadComplete = null)
        {
            string[] splitURL = downloadUrl.Split('/');
            string audicaName = splitURL[splitURL.Length - 1];
            string path = Path.Combine(SongBrowser.mainSongDirectory, audicaName);
            string downloadPath = Path.Combine(SongBrowser.downloadsDirectory, audicaName);
            if (!File.Exists(path) && !File.Exists(downloadPath))
            {
                WWW www = new WWW(downloadUrl);
                yield return www;
                byte[] results = www.bytes;
                File.WriteAllBytes(downloadPath, results);
            }
            yield return null;

            SongList.SongSourceDir dir = new SongList.SongSourceDir(Application.dataPath, SongBrowser.downloadsDirectory);
            string file = downloadPath.Replace('\\', '/');
            bool success = SongList.I.ProcessSingleSong(dir, file, new Il2CppSystem.Collections.Generic.HashSet<string>());
            downloadedFileNames.Add(audicaName);

            if (success)
            {
                needRefresh = true;
            }
            else
            {
                failedDownloads.Add(audicaName);
                if (File.Exists(downloadPath))
                    File.Delete(downloadPath);
            }

            onDownloadComplete?.Invoke(songID, success);
        }

        /// <summary>
        /// Coroutine that plays song preview for given preview URL.
        /// If called with the url of a preview that's already playing
        /// the preview will be stopped instead.
        /// </summary>
        /// <param name="url">URL to preview, typically Song.preview_url</param>
        public static IEnumerator StreamPreviewSong(string url)
        {
            if (lastPreview == url) // let people stop previews since they're very loud
            {
                lastPreview = "";
                player.Stop();
            }
            else
            {
                lastPreview = url;
                WWW www = new WWW(url);
                yield return www;
                if (www.isDone)
                {
                    Stream stream = new MemoryStream(www.bytes);
                    player.Stream = new OggDecodeStream(stream);
                    yield return new WaitForSeconds(0.2f);
                    player.Play();
                    yield return new WaitForSeconds(15f);
                }
            }

            yield return null;
        }

        internal static void StartNewSongSearch()
        {
            page = 1;
            StartNewPageSearch();
        }

        internal static void StartNewPageSearch()
        {
            SongDownloaderUI.ResetScrollPosition();
            MelonCoroutines.Start(DoSongWebSearch(searchString, (query, result) => {
                modList = result;
                if (SongDownloaderUI.songItemPanel != null)
                {
                    SongDownloaderUI.AddSongItems(SongDownloaderUI.songItemMenu, modList);
                }
            }, SongDownloaderUI.difficultyFilter,
               SongDownloaderUI.curated, SongDownloaderUI.popularity, page, false));
        }

        internal static void NextPage()
        {
            if (page > modList.total_pages)
                page = modList.total_pages;
            else if (page < 1)
                page = 1;
            else
                page++;
        }
        internal static void PreviousPage()
        {
            if (page == 1) return;
            if (page > modList.total_pages)
                page = modList.total_pages;
            else if (page < 1)
                page = 1;
            else
                page--;
        }
    }
    [Serializable]
    public class APIModList
    {
        public int total_pages;
        public int mod_count;
        public Mod[] mods;
        public int pagesize;
        public int page;
    }
}

