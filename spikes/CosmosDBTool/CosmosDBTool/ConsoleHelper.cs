using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDBTool
{
    class ConsoleHelper
    {
        public static void UpdateConsole(string message, List<string> _logger)
        {
            Console.Write(string.Format("\r{0}", "".PadLeft(Console.CursorLeft, ' ')));
            Console.Write(string.Format("\r{0}", message));

            if (_logger.Count > 0)
            {
                _logger.Add(Environment.NewLine);
            }
            _logger.Add($"*******       {message}       *******");

        }
       
    }
}
