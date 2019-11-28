using DiscordAbstraction;
using DiscordAbstraction.Extensions;
using DiscordAbstraction.Interfaces;
using ElizaBot.DatabaseContexts;
using ElizaBot.Handlers;
using ElizaBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection, settings);

            MigrateDatabase(settings);

            return ClientServiceBuilder.CreateDefaultBuilder(serviceCollection: serviceCollection)
                .UseToken(settings.BotToken)
                .UseEventHandler(options =>
                {
                    options.UseCommandsWithPrefix(settings.Prefix);
                    options.UseLogHandler(EventHandlers.LogHandler);
                }).Build();
        }

        private static void MigrateDatabase(AppSettings settings)
        {
            var builder = new DbContextOptionsBuilder<ApplicationContext>();
            builder.UseSqlite(settings.ConnectionString);

            using var context = new ApplicationContext(builder.Options);
            context.Database.Migrate();
        }

        private static void ConfigureServices(IServiceCollection services, AppSettings settings)
        {
            services.AddSingleton<RNGService>();
            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlite(settings.ConnectionString);
            });
        }
    }
}
