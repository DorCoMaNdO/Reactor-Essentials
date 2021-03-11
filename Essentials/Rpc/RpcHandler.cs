using BepInEx.IL2CPP;
using Hazel;
using Reactor;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Essentials.Rpc
{
    public static class RpcHandler // Exists separately because types can be omitted when inferred from usage.
    {
        private static BinaryFormatter BinaryFormatter = new BinaryFormatter();

        public static RpcHandler<TPlugin, TData> Register<TPlugin, TData>(TPlugin plugin, Action<MessageWriter, TData> serializeRpc,
            Func<MessageReader, TData> deserializeRpc, Action<PlayerControl, TData> handleRpc, RpcLocalHandling localHandling = RpcLocalHandling.None)
            where TPlugin : BasePlugin
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin), "Plugin must be specified.");

            if (serializeRpc == null) throw new ArgumentNullException(nameof(serializeRpc), "Serialization method must be specified.");
            if (deserializeRpc == null) throw new ArgumentNullException(nameof(deserializeRpc), "Deserialization method must be specified.");
            if (handleRpc == null) throw new ArgumentNullException(nameof(handleRpc), "Method must be specified.");

            RpcHandler<TPlugin, TData> customRpc = new RpcHandler<TPlugin, TData>(plugin, serializeRpc, deserializeRpc, handleRpc, localHandling);

            customRpc.Register();

            return customRpc;
        }

        public static RpcHandler<TPlugin, TData> Register<TPlugin, TData>(TPlugin plugin, IRpcType<TData> rpcType,
            RpcLocalHandling localHandling = RpcLocalHandling.None) where TPlugin : BasePlugin
        {
            return Register(plugin, rpcType.SerializeRpc, rpcType.DeserializeRpc, rpcType.HandleRpc, localHandling);
        }

        public static RpcHandler<TPlugin, TData> Register<TPlugin, TData>(TPlugin plugin, Action<PlayerControl, TData> handleRpc,
            RpcLocalHandling localHandling = RpcLocalHandling.None) where TPlugin : BasePlugin
        {
            return Register(plugin, Serialize, Deserialize<TData>, handleRpc, localHandling);
        }

        public static RpcHandler<TPlugin, TData> Register<TPlugin, TData>(TPlugin plugin, Action<MessageWriter, object> serializeRpc,
            Func<MessageReader, Type, object> deserializeRpc, Action<PlayerControl, object> handleRpc, RpcLocalHandling localHandling = RpcLocalHandling.None)
            where TPlugin : BasePlugin
        {
            if (serializeRpc == null) throw new ArgumentNullException(nameof(serializeRpc), "Serialization method must be specified.");
            if (deserializeRpc == null) throw new ArgumentNullException(nameof(deserializeRpc), "Deserialization method must be specified.");
            if (handleRpc == null) throw new ArgumentNullException(nameof(handleRpc), "Method must be specified.");

            return Register(plugin, new Action<MessageWriter, TData>((writer, data) => serializeRpc(writer, data)),
                new Func<MessageReader, TData>((reader) => (TData)deserializeRpc(reader, typeof(TData))),
                new Action<PlayerControl, TData>((sender, data) => handleRpc(sender, data)), localHandling);
        }

        private static RpcHandler<TPlugin, TData> Register<TPlugin, TData>(TPlugin plugin, Action<MessageWriter, TData> serializeRpc,
            Func<MessageReader, TData> deserializeRpc, Action<PlayerControl, object> handleRpc, RpcLocalHandling localHandling = RpcLocalHandling.None)
            where TPlugin : BasePlugin
        {
            if (handleRpc == null) throw new ArgumentNullException(nameof(handleRpc), "Method must be specified.");

            return Register(plugin, serializeRpc, deserializeRpc,
                new Action<PlayerControl, TData>((sender, data) => handleRpc(sender, data)), localHandling);
        }

        public static RpcHandler<TPlugin, TData> RegisterHandler<TPlugin, TData>(TPlugin plugin, Action<PlayerControl, object> handleRpc,
            RpcLocalHandling localHandling = RpcLocalHandling.None) where TPlugin : BasePlugin
        {
            return Register(plugin, Serialize, Deserialize<TData>, handleRpc, localHandling);
        }

        /// <summary>
        /// Serializes type <typeparamref name="T"/> using BinaryFormatter.
        /// </summary>
        /// <typeparam name="T">Type to serialize, may need to be attributed with <see cref="SerializableAttribute"/></typeparam>
        /// <param name="writer"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException">Type is not supported.</exception>
        public static void Serialize<T>(MessageWriter writer, T data)
        {
            if (data == null) throw new ArgumentNullException($"Rpc data cannot be null.");

            using MemoryStream ms = new MemoryStream();

            try
            {
                BinaryFormatter.Serialize(ms, data);

                writer.WriteBytesAndSize(ms.ToArray());
            }
            catch
            {
                throw new ArgumentException($"Type {data.GetType().Name} is not supported by the serializer, try adding [SerializableAttribute].");
            }
        }

        /// <summary>
        /// Attempts to serialize type <typeparamref name="T"/> using BinaryFormatter.
        /// </summary>
        /// <typeparam name="T">Type to serialize</typeparam>
        /// <returns>Serialization success</returns>
        public static bool TrySerialize<T>(MessageWriter writer, T data)
        {
            try
            {
                Serialize(writer, data);

                return true;
            }
            catch (ArgumentNullException)
            {
            }
            catch (ArgumentException)
            {
            }

            return false;
        }

        /// <summary>
        /// Deserialize type <typeparamref name="T"/> using BinaryFormatter.
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <returns>Deserialization success</returns>
        public static T Deserialize<T>(MessageReader reader)
        {
            try
            {
                using MemoryStream ms = new MemoryStream(reader.ReadBytesAndSize());

                return (T)BinaryFormatter.Deserialize(ms);
            }
            catch
            {
                throw new ArgumentException($"Type {typeof(T).Name} is not supported by the deserializer, try adding [SerializableAttribute].");
            }
        }

        /// <summary>
        /// Attempts to deserialize type <typeparamref name="T"/> using BinaryFormatter.
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <returns>Deserialization success</returns>
        public static bool TryDeserialize<T>(MessageReader reader, out T data)
        {
            try
            {
                data = Deserialize<T>(reader);

                return true;
            }
            catch (ArgumentException)
            {
            }

            data = default;

            return false;
        }
    }

    public class RpcHandler<TPlugin, TData> where TPlugin : BasePlugin
    {
        private class CustomRpc : PlayerCustomRpc<TPlugin, TData>
        {
            private readonly Action<MessageWriter, TData> SerializeRpc;
            private readonly Func<MessageReader, TData> DeserializeRpc;
            private readonly Action<PlayerControl, TData> HandleRpc;

            private readonly RpcLocalHandling _localHandling;

            internal CustomRpc(TPlugin plugin, Action<MessageWriter, TData> serializeRpc, Func<MessageReader, TData> deserializeRpc,
                Action<PlayerControl, TData> handleRpc, RpcLocalHandling localHandling = RpcLocalHandling.None) : base(plugin)
            {
                SerializeRpc = serializeRpc;
                DeserializeRpc = deserializeRpc;
                HandleRpc = handleRpc;

                _localHandling = localHandling;
            }

            public override RpcLocalHandling LocalHandling { get { return _localHandling; } }

            public override void Write(MessageWriter writer, TData data)
            {
                SerializeRpc(writer, data);
            }

            public override TData Read(MessageReader reader)
            {
                return DeserializeRpc(reader);
            }

            public override void Handle(PlayerControl innerNetObject, TData data)
            {
                if (innerNetObject?.Data == null) return;

                HandleRpc(innerNetObject, data);
            }
        }

        private readonly CustomRpc CustomRpcHandler;

        protected internal RpcHandler(TPlugin plugin, Action<MessageWriter, TData> serializeRpc, Func<MessageReader, TData> deserializeRpc,
            Action<PlayerControl, TData> handleRpc, RpcLocalHandling localHandling = RpcLocalHandling.None)
        {
            CustomRpcHandler = new CustomRpc(plugin, serializeRpc, deserializeRpc, handleRpc, localHandling);
        }

        public void Send(TData data, bool immediately = false)
        {
            CustomRpcHandler.Send(data, immediately);
        }

        public void SendTo(int targetId, TData data)
        {
            CustomRpcHandler.SendTo(targetId, data);
        }

        internal void Register()
        {
            try
            {
                PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager.Register(CustomRpcHandler);
            }
            catch (Exception e) when (e.InnerException?.Message?.Contains("already registered", StringComparison.Ordinal) == true)
            {
                throw new ArgumentException("The same data type cannot be registered more than once per plugin.", nameof(TData));
            }
        }
    }
}