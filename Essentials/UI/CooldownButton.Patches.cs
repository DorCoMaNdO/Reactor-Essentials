using HarmonyLib;
using System;

namespace Essentials.UI
{
    [HarmonyPatch]
    public partial class CooldownButton
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        [HarmonyPostfix]
        private static void HudManagerUpdate()
        {
            Buttons.RemoveAll(button => button.IsDisposed);

            foreach (CooldownButton button in Buttons)
            {
                try
                {
                    button.CreateButton();

                    button.Update();
                }
                catch (Exception e)
                {
                    EssentialsPlugin.Logger.LogWarning($"An exception has occurred when creating or updating a button: {e}");
                }
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        [HarmonyPostfix]
        private static void HudManagerSetHudActive([HarmonyArgument(0)]bool isActive)
        {
            HudVisible = isActive; // Show/hide all buttons, as the game does.
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        [HarmonyPostfix]
        private static void MeetingHudStart()
        {
            foreach (CooldownButton button in Buttons) button.EndEffect(false); // End button effect early, TODO: Add property to pause effect duration instead.
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Method_37))] //WrapUp
        [HarmonyPostfix]
        private static void ExileControllerWrapUp()
        {
            if (!DestroyableSingleton<TutorialManager>.InstanceExists && ShipStatus.Instance.IsGameOverDueToDeath()) return;

            foreach (CooldownButton button in Buttons) button.ApplyCooldown(); // Set button on cooldown after exile screen, TODO: Add property to disable.
        }
    }
}