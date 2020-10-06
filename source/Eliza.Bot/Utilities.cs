using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;

namespace Eliza.Bot
{
    public static class Utilities
    {
        public static DiscordSocketClient CreateDicordWebsocketClient(BotSettings botSettings)
        {
            return new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                MessageCacheSize = botSettings.MessageCacheSize >= 0 ?
                    botSettings.MessageCacheSize :
                    throw new Exception($"{nameof(botSettings.MessageCacheSize)} must be set to a non negative integer."),
#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Warning
#endif
            });
        }

        public static CommandService CreateCommandService(BotSettings botSettings)
        {
            return new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = botSettings.CaseSensitiveComands,
                SeparatorChar = ' ',

#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Warning
#endif
            });
        }
    }
}
