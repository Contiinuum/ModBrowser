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
							MelonCoroutines.Start(ModDownloader.DeleteMod(mod));
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
					optionsMenu.scrollable.AddRow(modText);
                }
                if(!mod.isDownloaded || !mod.isUpdated)
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