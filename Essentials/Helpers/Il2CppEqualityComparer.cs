#if S20201209 || S20210305 // Backport Reactor API
using System;
using System.Collections.Generic;
using Il2CppSystem.Runtime.CompilerServices;

namespace Essentials.Helpers
{
    [Obsolete("Backported from Reactor API, when updating to 2021.3.31.3s or newer, use the Reactor namespace")]
    public sealed class Il2CppEqualityComparer<T> : IEqualityComparer<T> where T : Il2CppSystem.Object
    {
        private static Il2CppEqualityComparer<T> _instance;

        public static Il2CppEqualityComparer<T> Instance
        {
            get
            {
                _instance ??= new Il2CppEqualityComparer<T>();
                return _instance;
            }
        }

        private Il2CppEqualityComparer()
        {
        }

        public int GetHashCode(T value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }

        public bool Equals(T left, T right)
        {
            if (left == null || right == null)
            {
                return left == null && right == null;
            }

            return left.Equals(right);
        }
    }
}
#endif