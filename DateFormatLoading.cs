using ICities;
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
                // check for new/loaded game, map editor, or theme editor
                if (mode == LoadMode.NewGame             ||
                    mode == LoadMode.NewGameFromScenario ||
                    mode == LoadMode.LoadGame            ||
                    mode == LoadMode.NewMap              ||
                    mode == LoadMode.LoadMap             ||
                    mode == LoadMode.NewTheme            ||
                    mode == LoadMode.LoadTheme           )
                {
                    // if Date Reformatter mod is enabled, display message and return
                    ColossalFramework.Plugins.PluginManager.PluginInfo mod = HarmonyPatcher.GetMod(565071445UL);
                    if (mod != null && mod.isEnabled)
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
                    HarmonyPatcher.CreatePatches();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
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
                LogUtil.LogException(ex);
            }
        }
    }
}