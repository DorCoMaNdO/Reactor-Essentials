using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using Essentials.UI;
using HarmonyLib;
using Reactor;
using Reactor.Patches;
using System.Reflection;

namespace Essentials
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id, BepInDependency.DependencyFlags.HardDependency)]
    [ReactorPluginSide(PluginSide.Both)]
    public partial class EssentialsPlugin : BasePlugin
    {
        public const string Id = "com.comando.essentials";

        public static EssentialsPlugin Instance { get { return PluginSingleton<EssentialsPlugin>.Instance; } }

        internal static ManualLogSource Logger { get { return Instance.Log; } }

        internal Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            PluginSingleton<EssentialsPlugin>.Instance = this;

            Harmony.PatchAll();

            RegisterInIl2CppAttribute.Register();
            RegisterCustomRpcAttribute.Register(this);

            ReactorVersionShower.TextUpdated += (text) =>
            {
#if S20201209 || S20210305 || S202103313
                string txt = text.Text;
#else
                string txt = text.text;
#endif

                int index = txt.IndexOf('\n');
                txt = txt.Insert(index == -1 ? txt.Length - 1 : index, "\nEssentials " + typeof(EssentialsPlugin).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

#if S20201209 || S20210305 || S202103313
                text.Text = txt;
#else
                text.text = txt;
#endif
            };

            HudPosition.Load();
        }

#if S202103313
        /// <summary>
        /// Corrects the issue where players are unable to move after meetings in 2021.3.31.3s, may interrupt controller support.
        /// </summary>
        public static bool PatchCanMove { get; set; } = true;

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
        private static class PlayerControlCanMovePatch
        {
            public static bool Prefix(PlayerControl __instance, ref bool __result)
            {
                if (!PatchCanMove) return true;

                __result = __instance.moveable && !Minigame.Instance && (!DestroyableSingleton<HudManager>.InstanceExists || !DestroyableSingleton<HudManager>.Instance.Chat.IsOpen && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen) /*&& (!ControllerManager.Instance || !ControllerManager.Instance.IsUiControllerActive)*/ && (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped) && !MeetingHud.Instance && !CustomPlayerMenu.Instance && !ExileController.Instance && !IntroCutscene.Instance;

                return false;
            }
        }
#endif
    }
}