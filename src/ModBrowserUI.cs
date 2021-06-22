using System.Collections;
using System;
using TMPro;
using UnityEngine;
using MelonLoader;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModBrowser
{
	internal static class ModBrowserUI
	{


		private static OptionsMenu primaryMenu;
		private static GunButton backButton;

		private static Dictionary<Mod, OptionsMenuButton> downloadButtons = new Dictionary<Mod, OptionsMenuButton>();

		static public void AddPageButton(OptionsMenu optionsMenu, int col)
		{
			primaryMenu = optionsMenu;
			primaryMenu.AddButton(col, "Download Mods", new Action(() => {
				GoToModBrowserPanel();
			}), null, "Download and Update Mods for Audica");

		}

		public static void GoToModBrowserPanel()
		{

			if (backButton == null)
			{
				var button = GameObject.Find("menu/ShellPage_Settings/page/backParent/back");
				backButton = button.GetComponentInChildren<GunButton>();

				UnityEngine.Object.Destroy(button.GetComponentInChildren<Localizer>());
			}

			primaryMenu.ShowPage(OptionsMenu.Page.Customization);
			CleanUpPage(primaryMenu);
			AddButtons(primaryMenu);
			primaryMenu.screenTitle.text = "Mods";
		}

		private static void AddButtons(OptionsMenu optionsMenu)
		{
			if(ModDownloader.remainingRequests == 0)
            {
				var header = optionsMenu.AddHeader(0, "Oops..");
				optionsMenu.scrollable.AddRow(header.gameObject);

				var text = optionsMenu.AddTextBlock(0, "You reached the API limit. You can only delete mods for now.");
				var tmp = text.transform.GetChild(0).GetComponent<TextMeshPro>();
				tmp.fontSizeMax = 32;
				tmp.fontSizeMin = 8;
				optionsMenu.scrollable.AddRow(text.gameObject);

				var text2 = optionsMenu.AddTextBlock(0, "Restart your game and check again in 1 hour.");
				var tmp2 = text2.transform.GetChild(0).GetComponent<TextMeshPro>();
				tmp2.fontSizeMax = 32;
				tmp2.fontSizeMin = 8;
				optionsMenu.scrollable.AddRow(text2.gameObject);
            }
			foreach(Mod mod in Main.mods)
            {
				var modheader = optionsMenu.AddHeader(0, $"{mod.displayRepoName} <size=5>(by {mod.displayUserName})</size>");
				optionsMenu.scrollable.AddRow(modheader);
				var buttonList = new Il2CppSystem.Collections.Generic.List<GameObject>();
                if (mod.isDownloaded)
                {
					var deleteText = "Delete";
					var deleteButton = optionsMenu.AddButton
					(0,
					deleteText,
					new Action(() =>
					{
						if (mod.isDownloaded)
                        {
							ModDownloader.DisableMod(mod);
							RefreshPage();
						}
					}),
					null,
					"Delete " + mod.displayRepoName);
					deleteButton.button.destroyOnShot = true;
					deleteButton.button.doMeshExplosion = false;
					buttonList.Add(deleteButton.gameObject);
				}
				if (mod.isUpdated)
				{
					var modText = optionsMenu.AddTextBlock(1, "<color=\"green\">Up to date!</color>");
					var tmp = modText.transform.GetChild(0).GetComponent<TextMeshPro>();
					tmp.fontSizeMax = 32;
					tmp.fontSizeMin = 8;
					buttonList.Add(modText.gameObject);
				}
				else if(!mod.isDownloaded || !mod.isDownloaded)
                {
					if(ModDownloader.remainingRequests > 0)
                    {
						if (mod.downloadLink != null && mod.downloadLink.Length > 0)
						{
							var buttonText = "Download";
							if (mod.isDownloaded && !mod.isUpdated) buttonText = "Update";
							var modButton = optionsMenu.AddButton
							(1,
							buttonText,
							new Action(() =>
							{
								bool isUpdate = mod.isDownloaded ? true : false;
								MelonCoroutines.Start(ModDownloader.DownloadMod(mod, isUpdate));
								RefreshPage();
							}),
							null,
							mod.description);
							modButton.button.destroyOnShot = true;
							modButton.button.doMeshExplosion = false;
							buttonList.Add(modButton.gameObject);

						}
					}				
				}			
				optionsMenu.scrollable.AddRow(buttonList);
			}			
		}

		private static void RefreshPage()
        {
			CleanUpPage(primaryMenu);
			AddButtons(primaryMenu);
        }

		

		
		private static void CleanUpPage(OptionsMenu optionsMenu)
		{
			Transform optionsTransform = optionsMenu.transform;
			for (int i = 0; i < optionsTransform.childCount; i++)
			{
				Transform child = optionsTransform.GetChild(i);
				if (child.gameObject.name.Contains("(Clone)"))
				{
					GameObject.Destroy(child.gameObject);
				}
			}
			optionsMenu.mRows.Clear();
			optionsMenu.scrollable.ClearRows();
			optionsMenu.scrollable.mRows.Clear();
			downloadButtons.Clear();
		}
	}
}