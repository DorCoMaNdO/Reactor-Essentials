using HarmonyLib;

namespace Essentials.UI
{
    [HarmonyPatch]
    public partial class CooldownButton
    {
#if S20201209 || S20210305
        [HarmonyPatch(typeof(HudManager.CoShowIntro__d), nameof(HudManager.CoShowIntro__d.MoveNext))]
#elif S202103313
        [HarmonyPatch(typeof(HudManager._CoShowIntro_d__56), nameof(HudManager._CoShowIntro_d__56.MoveNext))]
#else
        [HarmonyPatch(typeof(HudManager.Nested_5), nameof(HudManager.Nested_5.MoveNext))]
#endif
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
            foreach (CooldownButton button in CooldownButtons) if (button.MeetingsEndEffect) button.EndEffect(false); // End button effect early.
        }

        //[HarmonyPatch(typeof(ExileController.Animate__d), nameof(ExileController.Animate__d.MoveNext))]
#if S20201209
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Method_37))] //WrapUp 2020.12.9s
#elif S20210305
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Method_24))] //WrapUp 2021.3.5s
#elif S202103313
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.GALOAPAFIMJ))] //WrapUp 2021.3.31.3s
#else
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
#endif
        [HarmonyPostfix]
        private static void ExileControllerWrapUp()
        {
            if (!DestroyableSingleton<TutorialManager>.InstanceExists && ShipStatus.Instance.IsGameOverDueToDeath()) return;

            foreach (CooldownButton button in CooldownButtons) if (button.CooldownAfterMeetings && !button.IsEffectActive) button.ApplyCooldown(); // Set button on cooldown after exile screen.
        }

        //game start/end to reset effects/cooldowns
    }
}