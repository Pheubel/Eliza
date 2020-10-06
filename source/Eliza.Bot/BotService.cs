using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Eliza.Bot
{
    public class BotService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly BotSettings _settings;
        private readonly ILogger<BotService> _logger;
        private readonly CommandHandler _handler;

        private IServiceScope? _scope;

        public BotService(IServiceProvider? provider,
                       DiscordSocketClient? client,
                       CommandService? commands,
                       BotSettings? botConfig,
                       ILogger<BotService> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settings = botConfig ?? throw new ArgumentNullException(nameof(botConfig));
            _logger = logger;

            _handler = new CommandHandler(_client, _commands, _settings, _logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            _logger?.LogInformation("Setting up discord bot for execution");

            IServiceScope? scope = default;
            try
            {
                _scope = _provider.CreateScope();

                _handler.Initialize(_scope);

                await _commands.AddModulesAsync(typeof(BotService).Assembly, _scope.ServiceProvider);

                await StartClientAsync(stoppingToken);
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                try
                {
                    await _client.LogoutAsync();
                }
                finally
                {
                    _client?.Dispose();
                    scope?.Dispose();
                }

                _logger.LogError(ex, "An exception was triggered during set-up.");

                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _client.StopAsync();

            return base.StopAsync(cancellationToken);
        }

        private async Task StartClientAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _client.LoginAsync(TokenType.Bot, _settings.BotToken);
                await _client.StartAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not connect to Discord. Exception: {e.Message}");
                throw;
            }
        }
        public override void Dispose()
        {
            _client.Dispose();
            _scope?.Dispose();
            ((IDisposable)_commands).Dispose();

            base.Dispose();
        }
    }
}
