using DiscordAbstraction;
using DiscordAbstraction.Extensions;
using DiscordAbstraction.Interfaces;
using ElizaBot.Services;
using System;
using System.Threading.Tasks;

namespace ElizaBot
{
    class Program
    {
        static async Task Main() => await BuildService().RunAsync();

        private static IClientService BuildService()
        {
            var settings = new AppSettingsLoader().LoadSettings<AppSettings>();

            return ClientServiceBuilder.CreateDefaultBuilder()
                .UseToken(settings.BotToken)
                .AddSingletonService<RNGService>()
                .AddSingletonService(settings)
                .UseEventHandler(options =>
                {
                    options.UseCommandsWithPrefix(settings.Prefix);
                }).Build();
        }
    }
}
