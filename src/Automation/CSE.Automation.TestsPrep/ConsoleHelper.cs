using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.TestsPrep
{
    class ConsoleHelper
    {
        public static void UpdateConsole(string message, bool interactiveRun = false)
        {
            if (interactiveRun)
            {
                Console.Write(string.Format("\r{0}", "".PadLeft(Console.CursorLeft, ' ')));
                Console.Write(string.Format("\r{0}", message));
            }
        }
       
    }
}
