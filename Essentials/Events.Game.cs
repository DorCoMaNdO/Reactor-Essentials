using Essentials.Extensions;
using HarmonyLib;
using System;

namespace Essentials
{
    [HarmonyPatch]
    public static partial class Events
    {
        public static event EventHandler<EventArgs> HudCreated;
        public static event EventHandler<EventArgs> OnHudUpdate;
        public static event EventHandler<EventArgs> HudUpdated;
        public static event EventHandler<HudStateChangedEventArgs> HudStateChanged;
        public static event EventHandler<ResolutionChangedEventArgs> ResolutionChanged;
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        [HarmonyPostfix]
        private static void HudManagerStart()
        {
            HudCreated?.SafeInvoke(HudManager.Instance, EventArgs.Empty, nameof(HudCreated));
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        private static class HudManagerUpdate
        {
            private static void Prefix()
            {
                OnHudUpdate?.SafeInvoke(HudManager.Instance, EventArgs.Empty, nameof(OnHudUpdate));
            }

            private static void Postfix()
            {
                HudUpdated?.SafeInvoke(HudManager.Instance, EventArgs.Empty, nameof(HudUpdated));
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        [HarmonyPostfix]
        private static void HudManagerSetHudActive([HarmonyArgument(0)] bool isActive)
        {
            HudStateChanged?.SafeInvoke(HudManager.Instance, new HudStateChangedEventArgs(isActive), nameof(HudStateChanged));
        }
        
        internal static void RaiseResolutionChanged(int oldPixelWidth, int oldPixelHeight, float oldWidth, float oldHeight)
        {
            ResolutionChanged?.SafeInvoke(HudManager.Instance, new ResolutionChangedEventArgs(oldPixelWidth, oldPixelHeight, oldWidth, oldHeight), nameof(ResolutionChanged));
        }
    }
}