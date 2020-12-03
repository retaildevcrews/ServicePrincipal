using System;
using System.Collections.Generic;
using System.Text;

namespace AzQueueTestTool.TestCases
{
    class ConsoleHelper
    {
        public static void UpdateConsole(string message)
        {
            Console.Write(string.Format("\r{0}", "".PadLeft(Console.CursorLeft, ' ')));
            Console.Write(string.Format("\r{0}", message));
        }
       
    }
}
