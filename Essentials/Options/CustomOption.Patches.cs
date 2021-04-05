using Essentials.UI;
using HarmonyLib;
using Reactor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Essentials.Options
{
    [HarmonyPatch]
    public partial class CustomOption
    {
        private static Scroller OptionsScroller;
        private static Vector3 LastScrollPosition;
        private static int defaultGameOptionsCount = 0;

        private static List<OptionBehaviour> GetGameOptions(float lowestY)
        {
            List<OptionBehaviour> options = new List<OptionBehaviour>();

            /*EssentialsPlugin.Logger.LogInfo($"toggles {Object.FindObjectsOfType<ToggleOption>().Count}");
            EssentialsPlugin.Logger.LogInfo($"numbers {Object.FindObjectsOfType<NumberOption>().Count}");
            EssentialsPlugin.Logger.LogInfo($"strings {Object.FindObjectsOfType<StringOption>().Count}");
            EssentialsPlugin.Logger.LogInfo($"keyvalues {Object.FindObjectsOfType<KeyValueOption>().Count}");*/
            ToggleOption toggleOption = Object.FindObjectsOfType<ToggleOption>().FirstOrDefault();
            NumberOption numberOption = Object.FindObjectsOfType<NumberOption>().FirstOrDefault();
            StringOption stringOption = Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            //KeyValueOption kvOption = Object.FindObjectsOfType<KeyValueOption>().FirstOrDefault();

            int i = 0;
            foreach (CustomOption option in Options)
            {
                if (option.GameSetting != null)
                {
                    option.GameSetting.gameObject.SetActive(option.MenuVisible);

                    options.Add(option.GameSetting);

                    continue;
                }

                if (option.Type == CustomOptionType.Toggle && AmongUsClient.Instance?.AmHost == true)
                {
                    if (toggleOption == null) continue;

                    ToggleOption toggle = Object.Instantiate(toggleOption, toggleOption.transform.parent);//.DontDestroy();

                    if (!option.OnGameOptionCreated(toggle))
                    {
                        toggle.Destroy();

                        continue;
                    }

                    options.Add(toggle);

                    if (Debug) EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                }
                else if (option.Type == CustomOptionType.Number)
                {
                    if (numberOption == null) continue;

                    NumberOption number = Object.Instantiate(numberOption, numberOption.transform.parent);//.DontDestroy();

                    if (!option.OnGameOptionCreated(number))
                    {
                        number.Destroy();

                        continue;
                    }

                    options.Add(number);

                    if (Debug) EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                }
                else if (option.Type == CustomOptionType.String || option.Type == CustomOptionType.Toggle && AmongUsClient.Instance?.AmHost != true)
                {
                    //if (option is IKeyValueOption)
                    //{
                    //    //if (kvOption == null) continue;

                    //    Transform parent = kvOption?.transform?.parent ?? toggleOption?.transform?.parent ?? numberOption?.transform?.parent ?? stringOption?.transform?.parent;

                    //    if (parent == null) continue;

                    //    KeyValueOption kv = kvOption == null ? new KeyValueOption().DontDestroy() : Object.Instantiate(kvOption);

                    //    if (kv == null) continue;

                    //    kv.transform.SetParent(/*kvOption.transform.*/parent);

                    //    option.OnGameOptionCreated(kv);

                    //    options.Add(kv);

                    //    if (Debug)EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                    //}

                    if (stringOption == null) continue;

                    StringOption str = Object.Instantiate(stringOption, stringOption.transform.parent);//.DontDestroy();

                    if (!option.OnGameOptionCreated(str))
                    {
                        str.Destroy();

                        continue;
                    }

                    options.Add(str);

                    if (Debug) EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                }

                if (!option.GameSetting) continue;

                if (option.MenuVisible) option.GameSetting.transform.localPosition = new Vector3(option.GameSetting.transform.localPosition.x, lowestY - ++i * 0.5F, option.GameSetting.transform.localPosition.z);

                option.GameSetting.gameObject.SetActive(option.MenuVisible);
            }

            return options;
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        [HarmonyPostfix]
        private static void GameOptionsMenuStart(GameOptionsMenu __instance)
        {
            List<OptionBehaviour> customOptions = GetGameOptions(__instance.GetComponentsInChildren<OptionBehaviour>().Min(option => option.transform.localPosition.y));
            OptionBehaviour[] defaultOptions = __instance.Children;

            defaultGameOptionsCount = defaultOptions.Length;

            __instance.Children = defaultOptions.Concat(customOptions).ToArray();
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        [HarmonyPostfix]
        private static void GameOptionsMenuUpdate(GameOptionsMenu __instance)
        {
            if (Options.Count > 0)
            {
                List<OptionBehaviour> options = __instance.Children.Take(defaultGameOptionsCount).ToList();

                float lowestY = options.Min(option => option.transform.localPosition.y);
                int i = 0;

                foreach (CustomOption option in Options)
                {
                    if (!option.GameSetting?.gameObject) continue;

                    option.GameSetting.gameObject.SetActive(option.MenuVisible);

                    if (option.MenuVisible)
                    {
                        option.GameSetting.transform.localPosition = new Vector3(option.GameSetting.transform.localPosition.x, lowestY - ++i * 0.5F, option.GameSetting.transform.localPosition.z);

                        options.Add(option.GameSetting);
                    }
                }

                __instance.Children = options.ToArray();
            }

            __instance.GetComponentInParent<Scroller>().YBounds.max = (__instance.Children.Length - 7) * 0.5F + 0.13F;
        }

        //[HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))] //ToHudString 2020.12.9s
        [HarmonyPatch]
        private static class GameOptionsDataPatch
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(GameOptionsData).GetMethods(typeof(string), typeof(int));
            }

            private static void Postfix(ref string __result)
            {
                int firstNewline = __result.IndexOf('\n');
                StringBuilder sb = new StringBuilder(ClearDefaultHudText ? __result.Substring(0, firstNewline + 1) : __result);

                if (ShamelessPlug) sb.AppendLine("[FF1111FF]DorCoMaNdO on GitHub/Twitter/Twitch[]");
                foreach (CustomOption option in Options) if (option.HudVisible) sb.AppendLine(option.ToString());

                __result = sb.ToString();

                string insert = ":";
                if (HudTextScroller && (HudManager.Instance?.GameSettings?.Height).GetValueOrDefault() + 0.02F > HudPosition.Height) insert = " (Scroll for more):";
                __result = __result.Insert(firstNewline, insert);

                // Remove last newline (for the scroller to not overscroll one line)
                __result = __result[0..^1];
            }
        }

        private static bool OnEnable(OptionBehaviour opt)
        {
            CustomOption customOption = Options.FirstOrDefault(option => option.GameSetting == opt);

            if (customOption == null) return true;

            customOption.OnGameOptionCreated(opt);

            return false;
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.OnEnable))]
        private static class ToggleOptionOnEnablePatch
        {
            private static bool Prefix(ToggleOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.OnEnable))]
        private static class NumberOptionOnEnablePatch
        {
            private static bool Prefix(NumberOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
        private static class StringOptionOnEnablePatch
        {
            private static bool Prefix(StringOption __instance)
            {
                return OnEnable(__instance);
            }
        }

        private static bool OnFixedUpdate(OptionBehaviour opt)
        {
            return !Options.Any(option => option.GameSetting == opt);
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.FixedUpdate))]
        private static class NumberOptionFixedUpdatePatch
        {
            private static bool Prefix(NumberOption __instance)
            {
                return OnFixedUpdate(__instance);
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
        private static class StringOptionFixedUpdatePatch
        {
            private static bool Prefix(StringOption __instance)
            {
                return OnFixedUpdate(__instance);
            }
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
        private class ToggleButtonPatch
        {
            public static bool Prefix(ToggleOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance);

                if (option is IToggleOption toggle) toggle.Toggle();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
        private class NumberOptionPatchIncrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance);

                if (option is INumberOption number) number.Increase();

                return true;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
        private class NumberOptionPatchDecrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance);

                if (option is INumberOption number) number.Decrease();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
        private class StringOptionPatchIncrease
        {
            public static bool Prefix(StringOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance);

                if (option is IStringOption str) str.Increase();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
        private class StringOptionPatchDecrease
        {
            public static bool Prefix(StringOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance);

                if (option is IStringOption str) str.Decrease();

                return option == null;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
        private class PlayerControlPatch
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance?.AmHost != true || PlayerControl.AllPlayerControls.Count < 2 || !PlayerControl.LocalPlayer) return;

                //Rpc.Send(Options.Where(o => o.SendRpc).Select(o => ((string, CustomOptionType, object))o).ToArray());
                foreach (CustomOption option in Options) if (option.SendRpc) Rpc.Instance.Send(option, true);
                //Rpc.Instance.Send(Options.Where(o => o.SendRpc).Select(o => ((int, CustomOptionType, object))o).ToArray());
            }
        }

        static CustomOption()
        {
            Events.OnHudUpdate += UpdateScroller;
        }

        private static void UpdateScroller(object sender, EventArgs e)
        {
            HudManager hudManager = (HudManager)sender;

            if (hudManager?.GameSettings?.transform == null) return;

            hudManager.GameSettings.scale = HudTextScale;

            const float XOffset = 0.066666F, YOffset = 0.1F;

            // Scroller disabled
            if (!HudTextScroller)
            {
                // Remove scroller if disabled late
                if (OptionsScroller != null)
                {
                    hudManager.GameSettings.transform.SetParent(OptionsScroller.transform.parent);
                    hudManager.GameSettings.transform.localPosition = new HudPosition(XOffset, YOffset, HudAlignment.TopLeft);

                    Object.Destroy(OptionsScroller);
                }

                return;
            }

            CreateScroller(hudManager);

            // Update visibility
            OptionsScroller.gameObject.SetActive(hudManager.GameSettings.gameObject.activeSelf);

            if (!OptionsScroller.gameObject.active) return;

            // Scroll range
            OptionsScroller.YBounds = new FloatRange(HudPosition.TopLeft.y, Mathf.Max(HudPosition.TopLeft.y, hudManager.GameSettings.Height - HudPosition.TopLeft.y + 0.02F));

            float x = HudPosition.TopLeft.x + XOffset;
            OptionsScroller.XBounds = new FloatRange(x, x);

            Vector3 pos = hudManager.GameSettings.transform.localPosition;
            if (pos.x != x) // Resolution updated
            {
                pos.x = x;

                hudManager.GameSettings.transform.localPosition = pos;
            }

            // Prevent scrolling when the player is interacting with a menu
            if (PlayerControl.LocalPlayer?.CanMove != true)
            {
                hudManager.GameSettings.transform.localPosition = LastScrollPosition.x == x ? LastScrollPosition : (LastScrollPosition = HudPosition.TopLeft + new Vector2(XOffset, 0));

                return;
            }

            // Don't save position if not ready
            if (hudManager.GameSettings.transform.localPosition.y < HudPosition.TopLeft.y) return;

            LastScrollPosition = hudManager.GameSettings.transform.localPosition;
        }

        private static void CreateScroller(HudManager hudManager)
        {
            if (OptionsScroller != null) return;

            OptionsScroller = new GameObject("OptionsScroller").AddComponent<Scroller>();
            OptionsScroller.transform.SetParent(hudManager.GameSettings.transform.parent);
            OptionsScroller.gameObject.layer = 5;

            OptionsScroller.transform.localScale = Vector3.one;
            OptionsScroller.allowX = false;
            OptionsScroller.allowY = true;
            OptionsScroller.active = true;
            OptionsScroller.velocity = new Vector2(0, 0);
            OptionsScroller.ScrollerYRange = new FloatRange(0, 0);
            OptionsScroller.enabled = true;

            OptionsScroller.Inner = hudManager.GameSettings.transform;
            hudManager.GameSettings.transform.SetParent(OptionsScroller.transform);
        }
    }
}