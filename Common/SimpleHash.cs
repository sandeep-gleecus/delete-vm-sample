using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Simple utility class for hashing strings
    /// </summary>
    public static class SimpleHash
    {
        /// <summary>
        /// Returns an SHA256 hash
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetHashSha256(string text)
        {
            byte[] bytes = UTF8Encoding.Unicode.GetBytes(text);
            byte[] hash;
            using (SHA256 shaManaged = new SHA256Managed())
            {
                hash = shaManaged.ComputeHash(bytes);
            }
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        /// <summary>
        /// Returns an SHA1 hash
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetHashSha1(string text)
        {
            byte[] bytes = UTF8Encoding.Unicode.GetBytes(text);
            byte[] hash;
            using (SHA1 shaManaged = new SHA1Managed())
            {
                hash = shaManaged.ComputeHash(bytes);
            }
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }
}
