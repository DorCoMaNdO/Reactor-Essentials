using System;
using UnhollowerBaseLib;
using UnityEngine;

namespace Essentials.Extensions
{
    public static class Extensions
    {
        public static bool CompareName(this GameObject a, GameObject b)
        {
            return a == b || a != null & b != null && a.name != null && b.name != null && a.name.Length == b.name.Length && a.name.Equals(b.name, StringComparison.Ordinal);
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
    }
}
