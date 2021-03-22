using HarmonyLib;

namespace Essentials.UI
{
    [HarmonyPatch]
    public partial class CooldownButton
    {
        [HarmonyPatch(typeof(HudManager.CoShowIntro__d), nameof(HudManager.CoShowIntro__d.MoveNext))]
        [HarmonyPostfix]
        private static void HudManagerCoShowIntro()
        {
            const float fadeTime = 0.2F;
            foreach (CooldownButton button in CooldownButtons) button.ApplyCooldown(button.InitialCooldownDuration - fadeTime); // Match start cooldown.
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        [HarmonyPostfix]
        private static void MeetingHudStart()
        {
            foreach (CooldownButton button in CooldownButtons) if (button.MeetingsEndEffect) button.EndEffect(false); // End button effect early, TODO: Add property to pause effect duration instead.
        }

        //[HarmonyPatch(typeof(ExileController.Animate__d), nameof(ExileController.Animate__d.MoveNext))]
#if S20201209
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Method_37))] //WrapUp 2020.12.9s
#elif S20210305
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Method_24))] //WrapUp 2021.3.5s
#endif
        [HarmonyPostfix]
        private static void ExileControllerWrapUp()
        {
            if (!DestroyableSingleton<TutorialManager>.InstanceExists && ShipStatus.Instance.IsGameOverDueToDeath()) return;

            foreach (CooldownButton button in CooldownButtons) if (button.CooldownAfterMeetings && !button.IsEffectActive) button.ApplyCooldown(); // Set button on cooldown after exile screen, TODO: Add property to disable.
        }

        //game start/end to reset effects/cooldowns
    }
}