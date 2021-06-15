using System;
using System.Linq;
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

        /// <summary>
        /// Attempt to cast IL2CPP object <paramref name="obj"/> to type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="obj">IL2CPP object to cast</param>
        /// <param name="cast"><typeparamref name="T"/>-casted object</param>
        /// <returns>Successful cast</returns>
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
        /// Gets translated text from <see cref="StringNames"/> (an instance of <see cref="TranslationController"/> has to exist).
        /// </summary>
        /// <param name="str">String name to retrieve</param>
        /// <param name="parts">Elements to pass for formatting</param>
        /// <returns>The translated value of <paramref name="str"/></returns>
        public static string GetText(this StringNames str, params object[] parts)
        {
            return DestroyableSingleton<TranslationController>.Instance.GetString(str, parts);
        }

        /// <summary>
        /// Gets translated text from <see cref="StringNames"/>, <paramref name="defaultStr"/> is returned if a translation or an instance of <see cref="TranslationController"/> does not exist.
        /// </summary>
        /// <param name="str">String name to retrieve</param>
        /// <param name="defaultStr">Default string when translation is missing</param>
        /// <param name="parts">Elements to pass for formatting</param>
        /// <returns>The translated value of <paramref name="str"/></returns>
        public static string GetTextWithDefault(this StringNames str, string defaultStr, params object[] parts)
        {
            return DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(str, defaultStr, parts);
        }

        /// <summary>
        /// An implementation of <see cref="TranslationController.GetString(StringNames, Il2CppReferenceArray{Il2CppSystem.Object})"/> that handles IL2CPP casting.
        /// </summary>
        /// <param name="translationController">An instance of <see cref="TranslationController"/></param>
        /// <param name="str">String name to retrieve</param>
        /// <param name="parts">Elements to pass for formatting</param>
        /// <returns>The translated value of <paramref name="str"/></returns>
        public static string GetString(this TranslationController translationController, StringNames str, params object[] parts)
        {
            return translationController != null ? translationController.GetString(str, parts.Select(p => (Il2CppSystem.Object)p).ToArray()) : "STRMISS";
        }

        /// <summary>
        /// An implementation of <see cref="TranslationController.GetString(StringNames, Il2CppReferenceArray{Il2CppSystem.Object})"/> that handles IL2CPP casting and default value when a translation does not exist.
        /// </summary>
        /// <param name="translationController">An instance of <see cref="TranslationController"/></param>
        /// <param name="str">String name to retrieve</param>
        /// <param name="defaultStr">Default string when translation is missing</param>
        /// <param name="parts">Elements to pass for formatting</param>
        /// <returns>The translated value of <paramref name="str"/></returns>
        public static string GetStringWithDefault(this TranslationController translationController, StringNames str, string defaultStr, params object[] parts)
        {
            string text = translationController.GetString(str, parts);

            return /*str == StringNames.NoTranslation &&*/ text.Equals("STRMISS", StringComparison.Ordinal) && !string.IsNullOrEmpty(defaultStr) ? string.Format(defaultStr, parts) : text;
        }

        /// <summary>
        /// Converts X, Y of <see cref="Vector3"/> to <see cref="Vector2"/>.
        /// </summary>
        public static Vector2 ToVector2(in this Vector3 vector)
        {
            return vector;
        }

        /// <summary>
        /// Converts X, Y of <see cref="Vector2"/> to <see cref="Vector3"/> with specified Z (default 0).
        /// </summary>
        public static Vector3 ToVector3(in this Vector2 vector, float z = 0F)
        {
            return new Vector3(vector.x, vector.y, z);
        }
    }
}