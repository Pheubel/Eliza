using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ElizaBot
{
    public class CommandHandler
    {
        private IServiceScope _scope;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly BotConfig _settings;
        private readonly ILogger _logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, BotConfig settings, ILogger logger = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _logger = logger;
        }

        public void Initialize(IServiceScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));

            _client.MessageReceived += HandleCommandAsync;

            _client.Log += LogAsync;
        }

        private Task LogAsync(LogMessage log)
        {
            _logger?.LogInformation(log.Message);
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            // only allow users to make use of commands
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot)
                return;

            int argumentPosition = 0;

            // determines if the message has the determined prefix
            if (!userMessage.HasStringPrefix(_settings.Prefix, ref argumentPosition) && !(_settings.UseMentionPrefix && userMessage.HasMentionPrefix(_client.CurrentUser, ref argumentPosition)))
                return;

            // set up the context used for commands
            var context = new SocketCommandContext(_client, userMessage);

            // execute the command
            var result = await _commands.ExecuteAsync(context, argumentPosition, _scope.ServiceProvider);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand && !string.IsNullOrWhiteSpace(result.ErrorReason))
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}