using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace ModBrowser
{
    public static class Integrations
    {
        public static bool songBrowserInstalled = false;

        public static void LoadIntegrations()
        {
            if (MelonHandler.Mods.Any(it => it.Assembly.GetName().Name == "SongBrowser"))
            {
                var scoreVersion = new Version(MelonHandler.Mods.First(it => it.Assembly.GetName().Name == "AuthorableModifiers").Info.Version);
                var lastUnsupportedVersion = new Version("2.4.1");
                var result = scoreVersion.CompareTo(lastUnsupportedVersion);
                if (result > 0)
                {
                    songBrowserInstalled = true;
                }
                else
                {
                    MelonLogger.LogWarning("Please update SongBrowser to use this mod.");
                }
            }
        }
    }
}
