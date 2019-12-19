using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using ElizaBot.DatabaseContexts;
using ElizaBot.Handlers;
using ElizaBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ElizaBot
{
    public sealed class ElizaBot : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly DiscordRestClient _restClient;
        private readonly CommandService _commands;
        private readonly BotConfig _config;

        private IServiceScope _scope;

        public ElizaBot(IServiceProvider provider,
                        DiscordSocketClient client,
                        DiscordRestClient restClient,
                        CommandService commands,
                        IOptions<BotConfig> botConfig)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _config = botConfig?.Value ?? throw new ArgumentNullException(nameof(botConfig));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            IServiceScope scope = default;
            try
            {
                _scope = _provider.CreateScope();

                await _commands.AddModulesAsync(typeof(ElizaBot).Assembly, _scope.ServiceProvider);

                EventHandlers.SubscribeTohandlers(_client, _commands,_config,_provider);

                await StartClient(stoppingToken);

                await Task.Delay(-1,stoppingToken);
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

                throw;
            }
        }

        private async Task StartClient(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _client.LoginAsync(TokenType.Bot, _config.BotToken);
                await _client.StartAsync();

                await _restClient.LoginAsync(TokenType.Bot, _config.BotToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
