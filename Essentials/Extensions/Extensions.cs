using System;
using UnhollowerBaseLib;
using UnityEngine;

namespace Essentials.Extensions
{
    public static class Extensions
    {
        public static bool CompareName(this GameObject a, GameObject b)
        {
            return a == b || /*a?.name?.Length == b?.name?.Length &&*/ string.Equals(a?.name, b?.name, StringComparison.Ordinal);
        }

        public static bool CompareName(this OptionBehaviour a, OptionBehaviour b)
        {
            return CompareName(a?.gameObject, b?.gameObject);
        }

        public static bool TryCastTo<T>(this Il2CppObjectBase obj, out T cast) where T : Il2CppObjectBase
        {
            cast = obj.TryCast<T>();

            return cast != null;
        }

        /// <summary>
        /// Safely invokes event handlers by catching exceptions. Should mainly be used in game patches to prevent an exception from causing the game to hang.
        /// </summary>
        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args) where T : EventArgs
        {
            if (eventHandler == null) return;

            Delegate[] handlers = eventHandler.GetInvocationList();
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    ((EventHandler<T>)handlers[i])?.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    EssentialsPlugin.Logger.LogWarning($"Exception in event handler index {i} for event type {eventHandler.GetType()}: {e}");
                }
            }
        }
    }
}