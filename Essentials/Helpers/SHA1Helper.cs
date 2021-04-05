using System;
using System.Security.Cryptography;
using System.Text;

namespace Essentials.Helpers
{
    internal static class SHA1Helper
    {
        public const int Length = 16;

        public static byte[] Create(string name)
        {
            byte[] buffer = new byte[Length];
            using (SHA1 algorithm = SHA1.Create()) Array.Copy(algorithm.ComputeHash(Encoding.UTF8.GetBytes(name)), 0, buffer, 0, Length);

            return buffer;
        }
    }
}
