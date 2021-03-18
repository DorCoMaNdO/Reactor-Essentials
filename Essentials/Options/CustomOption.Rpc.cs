using Essentials.Rpc;
using Reactor;
using System;
using System.Linq;

namespace Essentials.Options
{
    public partial class CustomOption
    {
        private static RpcHandler<EssentialsPlugin, (string, CustomOptionType, object)[]> Rpc = RpcHandler.Register<EssentialsPlugin, (string, CustomOptionType, object)[]>(PluginSingleton<EssentialsPlugin>.Instance, HandleRpc);

        public static implicit operator (string ID, CustomOptionType Type, object Value)(CustomOption option)
        {
            return (option.ID, option.Type, option.GetValue<object>());
        }

        internal static void HandleRpc(PlayerControl sender, (string, CustomOptionType, object)[] options)
        {
            if (sender?.Data == null) return;

            if (Debug) EssentialsPlugin.Logger.LogInfo($"{sender.Data.PlayerName} sent option(s):");
            foreach ((string ID, CustomOptionType Type, object Value) option in options)
            {
                CustomOption customOption = Options.FirstOrDefault(o => o.Type == option.Type && o.ID.Equals(option.ID, StringComparison.Ordinal));

                if (customOption == null)
                {
                    EssentialsPlugin.Logger.LogWarning($"Received option that could not be found: \"{option.ID}\" of type {option.Type}.");

                    continue;
                }

                if (Debug) EssentialsPlugin.Logger.LogInfo($"\"{option.ID}\" type: {option.Type}, value: {option.Value}, current value: {customOption.Value}");

                customOption.SetValue(option.Value, true);

                if (Debug) EssentialsPlugin.Logger.LogInfo($"\"{option.ID}\", set value: {customOption.Value}");
            }
        }
    }
}