using HarmonyLib;
using UnityEngine;

namespace Convert
{
    [HarmonyPatch]
    internal partial class ConvertPlugin
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
        [HarmonyPostfix]
        private static void PlayerControlStart()
        {
            Conversions = 0;
        }

#if S20201209 || S20210305
        [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
#elif S202103313 || S20210510
        [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__11), nameof(IntroCutscene._CoBegin_d__11.MoveNext))]
#elif S20210615 || UNOBFUSCATED
        [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__14), nameof(IntroCutscene._CoBegin_d__14.MoveNext))]
#else
        [HarmonyPatch(typeof(IntroCutscene.Nested_0), nameof(IntroCutscene.Nested_0.MoveNext))]
#endif
        [HarmonyPostfix]
        private static void IntroCutsceneCoBegin()
        {
            Conversions = 0;
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
        [HarmonyPostfix]
        private static void EndGameManagerStart()
        {
            Conversions = 0;
        }

        // Disallow SetInfected RPC when conversion count is at the limit.
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        [HarmonyPrefix]
        private static bool PlayerControlHandleRpc([HarmonyArgument(0)] byte callid)
        {
            return callid != (byte)RpcCalls.SetInfected || ImpostorConversions.GetValue() == 0 || Conversions < ImpostorConversions.GetValue();
        }

        // Apply convert cooldown after kill when "Conversion has kill cooldown" is enabled.
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        [HarmonyPostfix]
        private static void PlayerControlMurderPlayer(PlayerControl __instance)
        {
            if (!ConversionCooldown.GetValue() || !__instance || !__instance.AmOwner || !DestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown) return;

            ConvertButton.ApplyCooldown();
        }
    }
}