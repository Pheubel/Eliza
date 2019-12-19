using Discord.WebSocket;
using Discord;
using Discord.Rest;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace ElizaBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEliza(this IServiceCollection services)
        {
            services.AddSingleton(provider => BuildClient(provider));
            services.AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>());
            services.AddSingleton(provider => new DiscordRestClient(new DiscordRestConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Warning
#endif
            }));
            services.AddSingleton(provider => BuildCommandServicde(provider));

            services.AddHostedService<ElizaBot>();

            return services;
        }

        private static DiscordSocketClient BuildClient(IServiceProvider serviceProvider)
        {
            var configurations = serviceProvider.GetRequiredService<IOptions<BotConfig>>().Value;

            return new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                MessageCacheSize = configurations.MessageCacheSize >= 0 ?
                    configurations.MessageCacheSize :
                    throw new Exception($"{nameof(configurations.MessageCacheSize)} must be set to a non negative integer."),

#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Warning
#endif
            });
        }

        private static CommandService BuildCommandServicde(IServiceProvider serviceProvider)
        {
            var service = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                SeparatorChar = ' ',

#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Warning
#endif
            });

            return service;
        }
    }
}
