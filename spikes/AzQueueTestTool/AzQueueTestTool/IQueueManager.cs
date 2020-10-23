using System;
using System.Collections.Generic;
using System.Text;

namespace AzQueueTestTool
{
    internal interface IQueueManager
    {
        void AddMessage(string messageText);

        void AddBaseMessages(int messageCount = 100);

        void RetrieveMessage(int maxMessages = 100);
    }
}
