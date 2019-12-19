using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElizaBot.Handlers
{
    public static partial class EventHandlers
    {
        public static void SubscribeTohandlers(DiscordSocketClient client, CommandService commandService, BotConfig config, IServiceProvider serviceProvider)
        {
            client.Log += LogHandler;

            client.MessageReceived += async (message) => await CommandMessageHandler(client, message, commandService, config, serviceProvider);
        }

        private static async Task CommandMessageHandler(DiscordSocketClient client, SocketMessage message, CommandService commandService, BotConfig config, IServiceProvider serviceProvider)
        {
            // only allow users to make use of commands
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot)
                return;

            int argumentPosition = 0;

            // determines if the message has the determined prefix
            if (!userMessage.HasStringPrefix(config.Prefix, ref argumentPosition) && !(config.UseMentionPrefix && userMessage.HasMentionPrefix(client.CurrentUser, ref argumentPosition)))
                return;

            // set up the context used for commands
            var context = new SocketCommandContext(client, userMessage);

            // execute the command
            var result = await commandService.ExecuteAsync(context, argumentPosition, serviceProvider);

            await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
