using System;
using System.Reflection;
using HarmonyLib;
namespace ModBrowser
{
    internal static class Hooks
    {
        private static int buttonCount = 0;

        [HarmonyPatch(typeof(OptionsMenu), "AddButton", new Type[] { typeof(int), typeof(string), typeof(OptionsMenuButton.SelectedActionDelegate), typeof(OptionsMenuButton.IsCheckedDelegate), typeof(string), typeof(OptionsMenuButton), })]
        private static class AddButtonButton
        {
            private static void Postfix(OptionsMenu __instance, int col, string label, OptionsMenuButton.SelectedActionDelegate onSelected, OptionsMenuButton.IsCheckedDelegate isChecked)
            {
                if (__instance.mPage == OptionsMenu.Page.Main)
                {
                    buttonCount++;
                    if (buttonCount == 9)
                    {
                        ModBrowserUI.AddPageButton(__instance, 1);
                        //SongSearchScreen.SetMenu(__instance);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ShowPage", new Type[] { typeof(OptionsMenu.Page) })]
        private static class PatchShowOptionsPage
        {
            private static void Prefix(OptionsMenu __instance, OptionsMenu.Page page)
            {
                buttonCount = 0;
            }
           
        }

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class Patch2SetMenuState
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {              

                if (state == MenuState.State.MainPage)
                {
                    if (ModDownloader.showPopup)
                    {
                        ModDownloader.ShowQueuedPopup();
                       
                    }
                    if (Main.showRestartReminder)
                    {
                        Main.ShowRestartReminder();
                    }
                }
            }
        }
    }
}