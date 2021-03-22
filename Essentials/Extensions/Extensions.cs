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
        /// <typeparam name="T">Event arguments type</typeparam>
        /// <param name="eventHandler">Event to invoke</param>
        /// <param name="sender">Object invoking the event</param>
        /// <param name="args">Event arguments</param>
        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args) where T : EventArgs
        {
            SafeInvoke(eventHandler, sender, args, eventHandler.GetType().Name);
        }

        /// <summary>
        /// Safely invokes event handlers by catching exceptions. Should mainly be used in game patches to prevent an exception from causing the game to hang.
        /// </summary>
        /// <typeparam name="T">Event arguments type</typeparam>
        /// <param name="eventHandler">Event to invoke</param>
        /// <param name="sender">Object invoking the event</param>
        /// <param name="args">Event arguments</param>
        /// <param name="eventName">Event name (logged in errors)</param>
        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args, string eventName) where T : EventArgs
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
                    EssentialsPlugin.Logger.LogWarning($"Exception in event handler index {i} for event \"{eventName}\":\n{e}");
                }
            }
        }

        /// <summary>
        /// Gets translated strings from <see cref="StringNames"/> (an instance of <see cref="TranslationController"/> has to exist).
        /// </summary>
        /// <param name="str">String name to retrieve</param>
        /// <param name="parts">Elements to pass for formatting</param>
        /// <returns>The translated value of <see cref="str"/></returns>
        public static string GetText(this StringNames str, params object[] parts)
        {
            return DestroyableSingleton<TranslationController>.Instance?.GetString(str, (Il2CppReferenceArray<Il2CppSystem.Object>)parts) ?? "STRMISS";
        }

        /// <summary>
        /// Converts X, Y of <see cref="Vector3"/> to <see cref="Vector2"/>.
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        /// <summary>
        /// Converts X, Y of <see cref="Vector2"/> to <see cref="Vector3"/> (Z = 0).
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, vector.y);
        }
    }
}