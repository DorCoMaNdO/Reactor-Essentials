using Hazel;
using System;

namespace Essentials.Rpc
{
    public interface IRpcType<TData>
    {
        void SerializeRpc(MessageWriter writer, TData data);
        TData DeserializeRpc(MessageReader reader);
        void HandleRpc(PlayerControl sender, TData data);
    }
}