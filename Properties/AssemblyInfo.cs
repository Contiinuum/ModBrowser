using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using ModBrowser;

[assembly: AssemblyTitle(ModBrowser.Main.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(ModBrowser.Main.BuildInfo.Company)]
[assembly: AssemblyProduct(ModBrowser.Main.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + ModBrowser.Main.BuildInfo.Author)]
[assembly: AssemblyTrademark(ModBrowser.Main.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(ModBrowser.Main.BuildInfo.Version)]
[assembly: AssemblyFileVersion(ModBrowser.Main.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(ModBrowser.Main), ModBrowser.Main.BuildInfo.Name, ModBrowser.Main.BuildInfo.Version, ModBrowser.Main.BuildInfo.Author, ModBrowser.Main.BuildInfo.DownloadLink)]

// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]