using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Eliza.Bot
{
    internal class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly BotSettings _settings;
        private readonly ILogger<BotService> _logger;

        private IServiceScope? _scope;

        public CommandHandler(DiscordSocketClient client, CommandService commands, BotSettings settings, ILogger<BotService> logger)
        {
            _client = client;
            _commands = commands;
            _settings = settings;
            _logger = logger;
        }

        internal void Initialize(IServiceScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));

            BindEvents();
        }

        private void BindEvents()
        {
            _client.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        internal void UnbindEvents()
        {
            _client.Log -= LogAsync;
            _client.MessageReceived -= HandleCommandAsync;
        }

        private Task LogAsync(LogMessage log)
        {
            _logger.LogInformation(log.Message);
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
            var result = await _commands.ExecuteAsync(context, argumentPosition, _scope!.ServiceProvider);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand && !string.IsNullOrWhiteSpace(result.ErrorReason))
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}