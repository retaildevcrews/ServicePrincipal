using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
