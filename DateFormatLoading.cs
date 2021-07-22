using ICities;
using UnityEngine;
using System;

namespace DateFormat
{
    /// <summary>
    /// handle game loading and unloading
    /// </summary>
    /// <remarks>A new instance of DateFormatLoading is NOT created when loading a game from the Pause Menu.</remarks>
    public class DateFormatLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            // do base processing
            base.OnLevelLoaded(mode);

            try
            {
                // check for new or loaded game
                if (mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario || mode == LoadMode.LoadGame)
                {
                    // if Date Reformatter mod is enabled, display message and return
                    if (HarmonyPatcher.IsModEnabled(565071445L))
                    {
                        // create dialog panel
                        ExceptionPanel panel = ColossalFramework.UI.UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                        panel.SetMessage(
                            "Date Format",
                            "The Date Format mod is a replacement for the Date Reformatter mod.  " + Environment.NewLine + Environment.NewLine +
                            "Please unsubscribe from the Date Reformatter mod.",
                            false);

                        // do not initialize this mod
                        return;
                    }

                    // create the Harmony patches
                    if (!HarmonyPatcher.CreatePatches()) return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void OnLevelUnloading()
        {
            // do base processing
            base.OnLevelUnloading();

            try
            {
                // remove Harmony patches
                HarmonyPatcher.RemovePatches();
            }
            catch (System.IO.FileNotFoundException ex)
            {
                // ignore missing Harmony, rethrow all others
                if (!ex.FileName.ToUpper().Contains("HARMONY"))
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}