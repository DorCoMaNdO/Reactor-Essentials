using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Reactor;

namespace Essentials
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id, BepInDependency.DependencyFlags.HardDependency)]
    [ReactorPluginSide(PluginSide.ClientOnly)]
    public partial class EssentialsPlugin : BasePlugin
    {
        public const string Id = "com.comando.essentials";

        public static EssentialsPlugin Instance { get { return PluginSingleton<EssentialsPlugin>.Instance; } }

        private ManualLogSource logger;
        internal static ManualLogSource Logger { get { return Instance.logger; } }

        internal Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            logger = Log;

            Harmony.PatchAll();

            RegisterInIl2CppAttribute.Register();
            RegisterCustomRpcAttribute.Register(this);
        }
    }
}
