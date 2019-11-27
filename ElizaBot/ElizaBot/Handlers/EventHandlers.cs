using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElizaBot.Handlers
{
    public static partial class EventHandlers
    {
        public static Task LogHandler(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.Message);
            return Task.CompletedTask;
        }
    }
}
