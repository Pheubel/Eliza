using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ElizaBot
{
    public sealed class ElizaBot : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly BotConfig _settings;
        private readonly ILogger<ElizaBot> _logger;

        private IServiceScope _scope;

        private readonly CommandHandler _handler;

        public ElizaBot(IServiceProvider provider,
                       DiscordSocketClient client,
                       CommandService commands,
                       IOptions<BotConfig> botConfig,
                       ILogger<ElizaBot> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settings = botConfig?.Value ?? throw new ArgumentNullException(nameof(botConfig));
            _logger = logger;

            _handler = new CommandHandler(_client, _commands, _settings, logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            _logger?.LogInformation("Setting up DinkBot for execution");

            IServiceScope scope = default;
            try
            {
                _scope = _provider.CreateScope();

                _handler.Initialize(_scope);

                await _commands.AddModulesAsync(typeof(ElizaBot).Assembly, _scope.ServiceProvider);

                await StartClient(stoppingToken);

                await Task.Delay(-1, stoppingToken);
            }
            catch (Exception ex)
            {
                try
                {
                    await _client.LogoutAsync();
                }
                finally
                {
                    scope?.Dispose();
                }

                _logger.LogError(ex, "An exception was triggered during set-up.");

                throw;
            }
        }

        private async Task StartClient(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _client.LoginAsync(TokenType.Bot, _settings.BotToken);
                await _client.StartAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
