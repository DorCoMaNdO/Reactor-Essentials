using Essentials.Extensions;
using HarmonyLib;
using System;

namespace Essentials
{
    [HarmonyPatch]
    public static partial class Events
    {
        public static event EventHandler<EventArgs> HudUpdate;
        public static event EventHandler<HudStateChangedEventArgs> HudStateChanged;

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        [HarmonyPostfix]
        private static void HudManagerUpdate(HudManager __instance)
        {
            HudUpdate?.SafeInvoke(__instance, EventArgs.Empty, nameof(HudUpdate));
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        [HarmonyPostfix]
        private static void HudManagerSetHudActive(HudManager __instance, [HarmonyArgument(0)] bool isActive)
        {
            HudStateChanged?.SafeInvoke(__instance, new HudStateChangedEventArgs(isActive), nameof(HudUpdate));
        }
    }
}