using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AzQueueTestTool.TestCases.Queues
{
    public static class StringExtensions
    {
        public static string AddRandomString(this string baseString)
        {
            string path = Path.GetRandomFileName();
            return $"{baseString} : {path}";
        }
        public static string AddRandomStringToEmail(this string baseString)
        {
            string path = Path.GetRandomFileName().Replace(".","");
            return $"{baseString}.{path}";
        }

        public static string GenerateToken(this string baseString, int length=32)
        {
            using (RNGCryptoServiceProvider cryptRNG = new RNGCryptoServiceProvider())
            {
                byte[] tokenBuffer = new byte[length];
                cryptRNG.GetBytes(tokenBuffer);
                return Convert.ToBase64String(tokenBuffer);
            }
        }
    }
}
