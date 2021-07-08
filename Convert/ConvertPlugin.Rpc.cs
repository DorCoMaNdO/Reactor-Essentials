using Hazel;
using Reactor;
#if !S20201209 && !S20210305
using Reactor.Networking;
#endif

namespace Convert
{
    internal partial class ConvertPlugin
    {
#if S20201209 || S20210305
        [RegisterCustomRpc]
#else
        [RegisterCustomRpc(0)]
#endif
        private class Rpc : PlayerCustomRpc<ConvertPlugin, byte>
        {
            public static Rpc Instance { get { return Rpc<Rpc>.Instance; } }

#if S20201209 || S20210305
            public Rpc(ConvertPlugin plugin) : base(plugin)
#else
            public Rpc(ConvertPlugin plugin, uint id) : base(plugin, id)
#endif
            {
            }

            public override RpcLocalHandling LocalHandling { get { return RpcLocalHandling.After; } }

            public override void Write(MessageWriter writer, byte newImpostor)
            {
                writer.Write(newImpostor);
            }

            public override byte Read(MessageReader reader)
            {
                return reader.ReadByte();
            }

            public override void Handle(PlayerControl innerNetObject, byte impostor)
            {
                if (innerNetObject?.Data == null) return;

                Conversions++;

                PluginSingleton<ConvertPlugin>.Instance.Log.LogInfo($"{innerNetObject.Data.PlayerName} reported new impostor: {impostor} {GameData.Instance.GetPlayerById(impostor).PlayerName}, conversions: {Conversions}");

                UpdateImpostors(impostor);
            }
        }
    }
}