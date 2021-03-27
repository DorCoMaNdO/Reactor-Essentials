using Hazel;
using Reactor;
using System;
using System.Linq;

namespace Essentials.Options
{
    public partial class CustomOption
    {
        /*private static RpcHandler<EssentialsPlugin, (string, CustomOptionType, object)[]> Rpc = RpcHandler.Register<EssentialsPlugin, (string, CustomOptionType, object)[]>(PluginSingleton<EssentialsPlugin>.Instance, HandleRpc);

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
        }*/

        [RegisterCustomRpc]
        private protected class Rpc : PlayerCustomRpc<EssentialsPlugin, (string, CustomOptionType, object)>
        {
            public static Rpc Instance { get { return Rpc<Rpc>.Instance; } }

            public Rpc(EssentialsPlugin plugin) : base(plugin)
            {
            }

            public override RpcLocalHandling LocalHandling { get { return RpcLocalHandling.None; } }

            public override void Write(MessageWriter writer, (string, CustomOptionType, object) option)
            {
                writer.Write(option.Item1); // ID
                writer.Write((int)option.Item2); // Type
                if (option.Item2 == CustomOptionType.Toggle) writer.Write((bool)option.Item3);
                else if (option.Item2 == CustomOptionType.Number) writer.Write((float)option.Item3);
                else if (option.Item2 == CustomOptionType.String) writer.Write((int)option.Item3);
            }

            public override (string, CustomOptionType, object) Read(MessageReader reader)
            {
                string id = reader.ReadString();
                CustomOptionType type = (CustomOptionType)reader.ReadInt32();
                object value = null;
                if (type == CustomOptionType.Toggle) value = reader.ReadBoolean();
                else if (type == CustomOptionType.Number) value = reader.ReadSingle();
                else if (type == CustomOptionType.String) value = reader.ReadInt32();

                return (id, type, value);
            }

            public override void Handle(PlayerControl sender, (string, CustomOptionType, object) option)
            {
                if (sender?.Data == null) return;

                string id = option.Item1;
                CustomOptionType type = option.Item2;
                CustomOption customOption = Options.FirstOrDefault(o => o.Type == type && o.ID.Equals(id, StringComparison.Ordinal));

                if (customOption == null)
                {
                    EssentialsPlugin.Logger.LogWarning($"Received option that could not be found: \"{id}\" of type {type}.");

                    return;
                }

                object value = option.Item3;

                if (Debug) EssentialsPlugin.Logger.LogInfo($"\"{id}\" type: {type}, value: {value}, current value: {customOption.Value}");

                customOption.SetValue(value, true);

                if (Debug) EssentialsPlugin.Logger.LogInfo($"\"{id}\", set value: {customOption.Value}");
            }
        }

        public static implicit operator (string ID, CustomOptionType Type, object Value)(CustomOption option)
        {
            return (option.ID, option.Type, option.GetValue<object>());
        }
    }
}