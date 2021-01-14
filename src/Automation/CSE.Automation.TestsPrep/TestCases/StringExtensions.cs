using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CSE.Automation.Model.Validators;

namespace CSE.Automation.TestsPrep.TestCases
{
    internal static class StringExtensions
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

        public static List<string> GetAsList(this string notes)
        {
            return notes.Split(ServicePrincipalModelValidator.NotesSeparators).Select(x => x.Trim()).ToList();
        }
    }
}
