using HarmonyLib;
using Reactor.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Essentials.CustomOptions
{
    public partial class CustomOption
    {
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
                    options.Add(option.GameSetting);

                    continue;
                }

                if (option.Type == CustomOptionType.Toggle)
                {
                    if (toggleOption == null) continue;

                    ToggleOption toggle = Object.Instantiate(toggleOption, toggleOption.transform.parent).DontDestroy();

                    toggle.transform.localPosition = new Vector3(toggle.transform.localPosition.x, lowestY - ++i * 0.5F, toggle.transform.localPosition.z);

                    option.OnGameOptionCreated(toggle);

                    options.Add(toggle);

                    EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                }
                else if (option.Type == CustomOptionType.Number)
                {
                    if (numberOption == null) continue;

                    NumberOption number = Object.Instantiate(numberOption, numberOption.transform.parent).DontDestroy();

                    number.transform.localPosition = new Vector3(number.transform.localPosition.x, lowestY - ++i * 0.5F, number.transform.localPosition.z);

                    option.OnGameOptionCreated(number);

                    options.Add(number);

                    EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                }
                else if (option.Type == CustomOptionType.String)
                {
                    //if (option is CustomKeyValueOption)
                    //{
                    //    //if (kvOption == null) continue;

                    //    Transform parent = kvOption?.transform?.parent ?? toggleOption?.transform?.parent ?? numberOption?.transform?.parent ?? stringOption?.transform?.parent;

                    //    if (parent == null) continue;

                    //    KeyValueOption kv = kvOption == null ? new KeyValueOption().DontDestroy() : Object.Instantiate(kvOption);

                    //    if (kv == null) continue;

                    //    kv.transform.SetParent(/*kvOption.transform.*/parent);

                    //    option.OnGameOptionCreated(kv);

                    //    options.Add(kv);

                    //    EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                    //}

                    if (stringOption == null) continue;

                    StringOption str = Object.Instantiate(stringOption, stringOption.transform.parent).DontDestroy();

                    str.transform.localPosition = new Vector3(str.transform.localPosition.x, lowestY - ++i * 0.5F, str.transform.localPosition.z);

                    option.OnGameOptionCreated(str);

                    options.Add(str);

                    EssentialsPlugin.Logger.LogInfo($"Option \"{option.Name}\" was created");
                }
            }

            return options;
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        private class GameOptionsMenuPatchStart
        {
            public static void Postfix(GameOptionsMenu __instance)
            {
                List<OptionBehaviour> customOptions = GetGameOptions(__instance.GetComponentsInChildren<OptionBehaviour>().Min(option => option.transform.localPosition.y));
                Il2CppReferenceArray<OptionBehaviour> defaultOptions = __instance.Children;

                OptionBehaviour[] options = defaultOptions.Concat(customOptions).ToArray();

                //EssentialsPlugin.Logger.LogInfo($"__instance.Children.Count {__instance.Children.Count}");

                __instance.Children = new Il2CppReferenceArray<OptionBehaviour>(options);

                //__instance.GetComponentInParent<Scroller>().YBounds.max = options.Length * /*0.3455F*/0.4F;
                //__instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + options.Length * 0.4F;

                //EssentialsPlugin.Logger.LogInfo($"__instance.Children.Count2 {__instance.Children.Count}");
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        private class GameOptionsMenuPatchUpdate
        {
            public static void Postfix(GameOptionsMenu __instance)
            {
                __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.Children.Length * 0.4F;
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))] //ToHudString
        private static class GameOptionsDataPatch
        {
            private static void Postfix(ref string __result)
            {
                StringBuilder builder = new StringBuilder(__result);
                if (ShamelessPlug) builder.AppendLine("[FF0000FF]DorCoMaNdO on GitHub/Twitter/Twitch[FFFFFFFF]");
                foreach (CustomOption option in Options) builder.AppendLine($"{option.Name}: {option}");

                __result = builder.ToString();
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

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
        private class ToggleButtonPatch
        {
            public static bool Prefix(ToggleOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomToggleOption toggle)
                {
                    toggle.Toggle();

                    //EssentialsPlugin.Logger.LogInfo($"Option \"{toggle.Name}\" was {toggle.GetOldValue()} now {toggle.GetValue()}");

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
        private class NumberOptionPatchIncrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomNumberOption number)
                {
                    number.Increase();

                    //EssentialsPlugin.Logger.LogInfo($"Option \"{number.Name}\" was {number.GetOldValue()} now {number.GetValue()}");

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
        private class NumberOptionPatchDecrease
        {
            public static bool Prefix(NumberOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance); // Works but may need to change to gameObject.name check
                if (option is CustomNumberOption number)
                {
                    number.Decrease();

                    //EssentialsPlugin.Logger.LogInfo($"Option \"{number.Name}\" was {number.GetOldValue()} now {number.GetValue()}");

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
        private class StringOptionPatchIncrease
        {
            public static bool Prefix(StringOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance);
                if (option is CustomStringOption str)
                {
                    str.Increase();

                    //EssentialsPlugin.Logger.LogInfo($"Option \"{str.Name}\" was \"{str.GetText(str.GetOldValue())}\" now \"{str.GetText()}\"");

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
        private class StringOptionPatchDecrease
        {
            public static bool Prefix(StringOption __instance)
            {
                CustomOption option = Options.FirstOrDefault(option => option.GameSetting == __instance);
                if (option is CustomStringOption str)
                {
                    string oldValue = str.GetText();

                    str.Decrease();

                    //EssentialsPlugin.Logger.LogInfo($"Option \"{str.Name}\" was \"{str.GetText(str.GetOldValue())}\" now \"{str.GetText()}\"");

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
        private class PlayerControlPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.AllPlayerControls.Count < 2 || !AmongUsClient.Instance || !PlayerControl.LocalPlayer || !AmongUsClient.Instance.AmHost) return;

                PlayerControl.LocalPlayer.Send<Rpc>(new Rpc.Data(Options));
            }
        }
    }
}
