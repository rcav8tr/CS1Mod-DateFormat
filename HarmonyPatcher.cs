using UnityEngine;
using CitiesHarmony.API;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using ColossalFramework.Plugins;
using ColossalFramework.UI;

namespace DateFormat
{
    /// <summary>
    /// Harmony patching
    /// </summary>
    internal static class HarmonyPatcher
    {
        // Harmony ID unique to this mod
        private const string HarmonyId = "com.github.rcav8tr.DateFormat";

        // whether or not Harmony patches were applied
        private static bool Patched = false;

        /// <summary>
        /// create Harmony patches
        /// </summary>
        public static void CreatePatches()
        {
            try
            {
                // not patched
                Patched = false;

                // check Harmony
                if (!HarmonyHelper.IsHarmonyInstalled)
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Missing Dependency",
                        "The Date Format mod requires the 'Harmony (Mod Dependency)' mod.  " + Environment.NewLine + Environment.NewLine +
                        "Please subscribe to the 'Harmony (Mod Dependency)' mod and restart the game.", error: false);
                    return;
                }

                // patch each method from the game that has a hard-coded date format
                CreateTranspilerPatch(typeof(UIDateTimeWrapper      ),"Check",                 BindingFlags.Public    | BindingFlags.Instance, out MethodInfo _);    // main game date
                CreateTranspilerPatch(typeof(ChirpXPanel            ),"UpdateBindings",        BindingFlags.NonPublic | BindingFlags.Instance, out MethodInfo _);
                CreateTranspilerPatch(typeof(FestivalPanel          ),"RefreshCurrentConcert", BindingFlags.NonPublic | BindingFlags.Instance, out MethodInfo _);
                CreateTranspilerPatch(typeof(FestivalPanel          ),"RefreshFutureConcert",  BindingFlags.NonPublic | BindingFlags.Instance, out MethodInfo _);
                CreateTranspilerPatch(typeof(FootballPanel          ),"RefreshMatchInfo",      BindingFlags.NonPublic | BindingFlags.Instance, out MethodInfo _);
                CreateTranspilerPatch(typeof(VarsitySportsArenaPanel),"RefreshPastMatches",    BindingFlags.NonPublic | BindingFlags.Instance, out MethodInfo _);
                CreateTranspilerPatch(typeof(VarsitySportsArenaPanel),"RefreshNextMatchDates", BindingFlags.NonPublic | BindingFlags.Instance, out MethodInfo _);

                // patch Extended InfoPanel mod; original version that has by far the most subsribers
                bool patchedExtendedInfoPanel1 = CreateTranspilerPatchForMod(
                    781767563UL,
                    "IINS.ExtendedInfo",
                    "IINS.ExtendedInfo",
                    "CityInfoDatas",
                    "UpdateDate_1",
                    BindingFlags.Public | BindingFlags.Instance,
                    out PluginManager.PluginInfo modExtendedInfoPanel1,
                    out Type originalTypeExtendedInfoPanel1,
                    out MethodInfo originalMethodExtendedInfoPanel1);

                // patch Extended InfoPanel 21:9 mod; has exactly the same signature as the original version above, just a different Steam ID
                bool patchedExtendedInfoPanel219 = CreateTranspilerPatchForMod(
                    2274354659UL,
                    "IINS.ExtendedInfo",
                    "IINS.ExtendedInfo",
                    "CityInfoDatas",
                    "UpdateDate_1",
                    BindingFlags.Public | BindingFlags.Instance,
                    out PluginManager.PluginInfo modExtendedInfoPanel219,
                    out Type originalTypeExtendedInfoPanel219,
                    out MethodInfo originalMethodExtendedInfoPanel219);

                // patch Extended InfoPanel 2 mod; has a different assembly name than the versions above
                bool patchedExtendedInfoPanel2 = CreateTranspilerPatchForMod(
                    2498761388UL,
                    "ExtendedInfo2",
                    "IINS.ExtendedInfo",
                    "CityInfoDatas",
                    "UpdateDate_1",
                    BindingFlags.Public | BindingFlags.Instance,
                    out PluginManager.PluginInfo modExtendedInfoPanel2,
                    out Type originalTypeExtendedInfoPanel2,
                    out MethodInfo originalMethodExtendedInfoPanel2);

                // patch Enhanced Outside Connections View mod
                bool patchedEnhancedOutsideConnectionView = CreateTranspilerPatchForMod(
                    2368396560UL,
                    "EnhancedOutsideConnectionsView",
                    "EnhancedOutsideConnectionsView",
                    "EOCVGraph",
                    "OnTooltipHover",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    out PluginManager.PluginInfo _,
                    out Type _,
                    out MethodInfo _);

                // patch More City Statistics mod; 2 patches
                bool patchedMoreCityStatistics1 = CreateTranspilerPatchForMod(
                    2685974449UL,
                    "MoreCityStatistics",
                    "MoreCityStatistics",
                    "UIImprovedGraph",
                    "OnTooltipHover",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    out PluginManager.PluginInfo _,
                    out Type _,
                    out MethodInfo _);
                bool patchedMoreCityStatistics2 = CreateTranspilerPatchForMod(
                    2685974449UL,
                    "MoreCityStatistics",
                    "MoreCityStatistics",
                    "ShowRange",
                    "UpdateSliderLabel",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    out PluginManager.PluginInfo _,
                    out Type originalTypeMoreCityStatistics2,
                    out MethodInfo _);

                // patch Real Time mod
                bool patchedRealTime = CreateTranspilerPatchForMod(
                    1420955187UL,
                    "RealTime",
                    "RealTime.UI",
                    "DateTooltipBehavior",
                    "UpdateTooltip",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    out PluginManager.PluginInfo modRealTime,
                    out Type originalTypeRealTime,
                    out MethodInfo originalMethodRealTime);

                // always refresh main game date
                RefreshMainGameDate();

                // refresh dates for patched mods that need it
                if (patchedExtendedInfoPanel1  ) { RefreshDateExtendedInfoPanel(modExtendedInfoPanel1,   originalTypeExtendedInfoPanel1,   originalMethodExtendedInfoPanel1  ); }
                if (patchedExtendedInfoPanel219) { RefreshDateExtendedInfoPanel(modExtendedInfoPanel219, originalTypeExtendedInfoPanel219, originalMethodExtendedInfoPanel219); }
                if (patchedExtendedInfoPanel2  ) { RefreshDateExtendedInfoPanel(modExtendedInfoPanel2,   originalTypeExtendedInfoPanel2,   originalMethodExtendedInfoPanel2  ); }
                if (patchedMoreCityStatistics2 ) { RefreshDateMoreCityStatistics(originalTypeMoreCityStatistics2); }
                if (patchedRealTime            ) { RefreshDateRealTime(modRealTime, originalTypeRealTime, originalMethodRealTime); }

                // success
                Patched = true;
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }

        /// <summary>
        /// create a transpiler patch based on type and method name
        /// </summary>
        private static bool CreateTranspilerPatch(Type originalType, string originalMethodName, BindingFlags bindingFlags, out MethodInfo originalMethod)
        {
            // initialize return value
            originalMethod = null;

            // validate original type
            if (originalType == null)
            {
                LogUtil.LogError($"Error patching method [{originalMethodName}].  Original type is null.");
                return false;
            }

            // build full name of method to be patched
            string fullMethodName = "[" + originalType.Assembly.GetName().Name + "." + originalType.FullName + "." + originalMethodName + "]";

            // don't allow an error here to prevent the rest of this mod from working
            try
            {
                // get the original method
                originalMethod = originalType.GetMethod(originalMethodName, bindingFlags);
                if (originalMethod == null)
                {
                    LogUtil.LogError($"Error patching {fullMethodName}.  Unable to find original method.");
                    return false;
                }

                // get the transpiler method
                MethodInfo transpilerMethod = typeof(HarmonyPatcher).GetMethod(nameof(ReplaceDateFormatString), BindingFlags.Static | BindingFlags.NonPublic);
                if (transpilerMethod == null)
                {
                    LogUtil.LogError($"Error patching {fullMethodName}.  Unable to find patch transpiler method HarmonyPatcher.ReplaceDateFormatString.");
                    return false;
                }

                // patch the original method using the tranpiler patch method
                new Harmony(HarmonyId).Patch(originalMethod, null, null, new HarmonyMethod(transpilerMethod));
                LogUtil.LogInfo($"Patched: {fullMethodName}.");
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Exception patching {fullMethodName}.");
                LogUtil.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// create a transpiler patch for a mod based on modID, assembly name, namespace name, type name, and method name
        /// </summary>
        private static bool CreateTranspilerPatchForMod(
            ulong modID,
            string assemblyName,
            string namespaceName,
            string typeName,
            string originalMethodName,
            BindingFlags bindingFlags,
            out PluginManager.PluginInfo mod,
            out Type originalType,
            out MethodInfo originalMethod)
        {
            // set default return values
            mod = null;
            originalType = null;
            originalMethod = null;

            // build full name of method to be patched
            string fullMethodName = "[" + assemblyName + "." + namespaceName + "." + typeName + "." + originalMethodName + "]";

            // don't allow an error here to prevent the rest of this mod from working
            try
            {
                // mod must be subscribed
                mod = GetMod(modID);
                if (mod == null)
                {
                    // this is not an error, just log an info message
                    LogUtil.LogInfo($"Mod [{modID}] is not subscribed.  Therefore {fullMethodName} is not patched.");
                    return false;
                }

                // mod must be enabled
                if (!mod.isEnabled)
                {
                    // this is not an error, just log an info message
                    LogUtil.LogInfo($"Mod [{modID}] is not enabled.  Therefore {fullMethodName} is not patched.");
                    return false;
                }

                // mod must have the named assembly
                Assembly originalAssembly = null;
                foreach (Assembly assembly in mod.GetAssemblies())
                {
                    if (assembly.GetName().Name == assemblyName)
                    {
                        // found it
                        originalAssembly = assembly;
                        break;
                    }
                }
                if (originalAssembly == null)
                {
                    LogUtil.LogError($"Error patching mod [{modID}] for {fullMethodName}.  Mod does not contain assembly.");
                    return false;
                }

                // assembly must have the named namespace and named type
                bool foundNamespace = false;
                foreach (Type type in originalAssembly.GetTypes())
                {
                    if (type.Namespace == namespaceName)
                    {
                        foundNamespace = true;
                        if (type.Name == typeName)
                        {
                            // found it
                            originalType = type;
                            break;
                        }
                    }
                }
                if (!foundNamespace)
                {
                    LogUtil.LogError($"Error patching mod [{modID}] for {fullMethodName}.  Assembly does not contain namespace.");
                    return false;
                }
                if (originalType == null)
                {
                    LogUtil.LogError($"Error patching mod [{modID}] for {fullMethodName}.  Namespace does not contain type.");
                    return false;
                }

                // got the original type, patch the method
                return CreateTranspilerPatch(originalType, originalMethodName, bindingFlags, out originalMethod);
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Exception patching mod [{modID}] for {fullMethodName}.");
                LogUtil.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// find and replace hard-coded date formats with the configured date format
        /// </summary>
        private static IEnumerable<CodeInstruction> ReplaceDateFormatString(IEnumerable<CodeInstruction> instructions)
        {
            // get the configured date format
            DateFormatConfiguration config = Configuration<DateFormatConfiguration>.Load();
            string dateFormat = config.BuildDateFormatString();

            // copy instructions to new instructions
            List<CodeInstruction> newInstructions = new List<CodeInstruction>(instructions);

            // find and replace all occurrences of "dd/MM/yyyy", "yyyy-MM-dd", and "d" with the configured date format
            foreach (CodeInstruction instruction in newInstructions)
            {
                if (instruction.opcode == System.Reflection.Emit.OpCodes.Ldstr)
                {
                    string operand = (string)instruction.operand;
                    if (operand == "dd/MM/yyyy" || operand == "yyyy-MM-dd" || operand == "d")
                    {
                        instruction.operand = dateFormat;
                    }
                }
            }

            // return the updated instructions
            return newInstructions;
        }

        /// <summary>
        /// refresh main game date display
        /// </summary>
        private static void RefreshMainGameDate()
        {
            // don't allow an error here to prevent the rest of this mod from working
            try
            {
                // in Bindings, set saved game time value field to max date/time so that next time UIDateTimeWrapper.Check is called
                // the main game date will be different than the saved value and the game date will be updated immediately even if the simulation is paused
                // this is instead of waiting for the game date to change due to the simulation running

                // get Bindings component
                Bindings bindings = UIView.GetAView().GetComponent<Bindings>();
                if (bindings == null)
                {
                    LogUtil.LogError("Error refreshing main game date.  Unable to get Bindings component.");
                    return;
                }

                // get Bindings.m_GameTime property
                FieldInfo fiGameTime = typeof(Bindings).GetField("m_GameTime", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fiGameTime == null)
                {
                    LogUtil.LogError("Error refreshing main game date.  Unable to get Bindings.m_GameTime property.");
                    return;
                }

                // get Bindings.m_GameTime value, which is a UIDateTimeWrapper
                UIDateTimeWrapper gameTime = fiGameTime.GetValue(bindings) as UIDateTimeWrapper;
                if (gameTime == null)
                {
                    LogUtil.LogError("Error refreshing main game date.  Unable to get Bindings.m_GameTime value.");
                    return;
                }

                // get UIDateTimeWrapper.m_Value field
                FieldInfo fiValue = typeof(UIDateTimeWrapper).GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fiValue == null)
                {
                    LogUtil.LogError("Error refreshing main game date.  Unable to get UIDateTimeWrapper.m_Value field.");
                    return;
                }

                // set Bindings.m_GameTime.m_Value to the max date/time
                fiValue.SetValue(gameTime, DateTime.MaxValue);
                LogUtil.LogInfo("Refreshed main game date.");
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }

        /// <summary>
        /// refresh game date displayed by the Extended Info Panel mods
        /// </summary>
        private static void RefreshDateExtendedInfoPanel(PluginManager.PluginInfo mod, Type originalType, MethodInfo originalMethod)
        {
            // don't allow an error here to prevent the rest of this mod from working
            try
            {
                // find original object
                UnityEngine.Object originalObject = UnityEngine.Object.FindObjectOfType(originalType);
                if (originalObject == null)
                {
                    LogUtil.LogError($"Error refreshing date for [{mod.name}] mod.  Unable to find object.");
                    return;
                }

                // call the original method on the original object
                originalMethod.Invoke(originalObject, null);
                LogUtil.LogInfo($"Refreshed date for [{mod.publishedFileID}] [{originalType.Assembly.GetName().Name}] mod.");
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }

        /// <summary>
        /// refresh date on sliders on More City Statistics mod graph
        /// </summary>
        private static void RefreshDateMoreCityStatistics(Type originalType)
        {
            // don't allow an error here to prevent the rest of this mod from working
            try
            {
                // call MoreCityStatistics.ShowRange.instance.UpdateSliderLabels to update slider labels
                // this method updates both slider labels and is a different method than the method that was patched

                // get MoreCityStatistics.ShowRange.instance property
                PropertyInfo property = originalType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public);
                if (property == null)
                {
                    LogUtil.LogError("Error refreshing date for MoreCityStatistics mod.  Unable to get MoreCityStatistics.ShowRange.instance property.");
                    return;
                }

                // get MoreCityStatistics.ShowRange.instance value
                object instance = property.GetValue(null, null);
                if (instance == null)
                {
                    LogUtil.LogError("Error refreshing date for MoreCityStatistics mod.  Unable to get MoreCityStatistics.ShowRange.instance value.");
                    return;
                }

                // get MoreCityStatistics.ShowRange.UpdateSliderLabels method
                MethodInfo method = originalType.GetMethod("UpdateSliderLabels", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                {
                    LogUtil.LogError("Error refreshing date for MoreCityStatistics mod.  Unable to get MoreCityStatistics.ShowRange.UpdateSliderLabels method.");
                    return;
                }

                // call the method on the instance
                method.Invoke(instance, null);
                LogUtil.LogInfo("Refreshed date for MoreCityStatistics mod.");
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }
        /// <summary>
        /// refresh game date displayed by RealTime mod
        /// </summary>
        private static void RefreshDateRealTime(PluginManager.PluginInfo mod, Type originalType, MethodInfo originalMethod)
        {
            // don't allow an error here to prevent the rest of this mod from working
            try
            {
                // find the game's InfoPanel
                UIPanel infoPanel = UIView.Find<UIPanel>("InfoPanel");
                if (infoPanel == null)
                {
                    LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to find InfoPanel.");
                    return;
                }

                // find PanelTime on InfoPanel
                UIPanel panelTime = infoPanel.Find<UIPanel>("PanelTime");
                if (panelTime == null)
                {
                    LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to find PanelTime.");
                    return;
                }

                // find Sprite on PanelTime
                UISprite sprite = panelTime.Find<UISprite>("Sprite");
                if (sprite == null)
                {
                    LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to find Sprite.");
                    return;
                }

                // get the DateTooltipBehavior attached to the progress sprite
                Component spriteDateTooltipBehavior = sprite.gameObject.GetComponent(originalType);
                if (spriteDateTooltipBehavior == null)
                {
                    // when a game is first started, Date Format might initialize before Real Time
                    // so DateTooltipBehavior will not be attached yet (i.e. component will not be found)

                    // get the Real Time assembly
                    Assembly assemblyRealTime = null;
                    foreach (Assembly assembly in mod.GetAssemblies())
                    {
                        if (assembly.GetName().Name == "RealTime")
                        {
                            assemblyRealTime = assembly;
                            break;
                        }
                    }
                    if (assemblyRealTime == null)
                    {
                        LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to get RealTime assembly.");
                        return;
                    }

                    // get RealTime.Core.RealTimeMod class from the assembly
                    Type mainType = null;
                    foreach (Type type in assemblyRealTime.GetTypes())
                    {
                        if (type.Namespace == "RealTime.Core" && type.Name == "RealTimeMod")
                        {
                            // found it
                            mainType = type;
                            break;
                        }
                    }
                    if (mainType == null)
                    {
                        LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to get RealTime.Core.RealTimeMod type.");
                        return;
                    }

                    // get the "core" field
                    FieldInfo fieldCore = mainType.GetField("core", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (fieldCore == null)
                    {
                        LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to get core field.");
                        return;
                    }

                    // get RealTime instance
                    ICities.IUserMod[] instances = mod.GetInstances<ICities.IUserMod>();
                    if (instances.Length != 1)
                    {
                        LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to get Real Time instance.");
                        return;
                    }

                    // check the core value
                    object coreValue = fieldCore.GetValue(instances[0]);
                    if (coreValue == null)
                    {
                        // the core value is null, which means that the RealTime mod is not yet initialized
                        // when the mod does eventually initialize, it will use the patched date format
                        // just log an info message
                        LogUtil.LogInfo("Date not refreshed for RealTime mod because the mod is not yet initialized.");
                    }
                    else
                    {
                        // core value is not null, which means the RealTime mod is initialized
                        // but the attached DateTooltipBehavior could not be found
                        // this is a real error
                        LogUtil.LogError("Error refreshing date for RealTime mod.  Unable to get DateTooltipBehavior component.");
                    }

                    // in either case, the method cannot be invoked without the DateTooltipBehavior component
                    return;
                }

                // invoke the patched method on the DateTooltipBehavior component, forcing an update of the tool tip
                originalMethod.Invoke(spriteDateTooltipBehavior, new object[] { true });
                LogUtil.LogInfo("Refreshed date for RealTime mod.");
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }

        /// <summary>
        /// remove Harmony patches
        /// </summary>
        public static void RemovePatches()
        {
            try
            {
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    new Harmony(HarmonyId).UnpatchAll(HarmonyId);
                    Patched = false;
                    LogUtil.LogInfo("All patches removed.");
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }

        /// <summary>
        /// remove and recreate patches
        /// </summary>
        public static void ReapplyPatches()
        {
            if (Patched)
            {
                // remove and recreate patches
                RemovePatches();
                CreatePatches();
            }
        }

        /// <summary>
        /// get the specified mod; return null if not found
        /// </summary>
        public static PluginManager.PluginInfo GetMod(ulong modID)
        {
            // do each mod
            foreach (PluginManager.PluginInfo mod in PluginManager.instance.GetPluginsInfo())
            {
                // check if this is the specified mod
                if (mod.publishedFileID.AsUInt64 == modID)
                {
                    // found the mod, return it
                    return mod;
                }
            }

            // not found; possibly because the mod is simply not subscribed
            return null;
        }
    }
}
