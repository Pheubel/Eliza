using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using Eliza.Client.Services.Core;
using Eliza.Client.Services;
using Eliza.Shared;

namespace Eliza.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<DiscordAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider, DiscordAuthenticationStateProvider>((provider) => provider.GetRequiredService<DiscordAuthenticationStateProvider>());
            builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
            builder.Services.AddScoped<ITagApi, TagApi>();

            builder.Services.AddAuthorizationCore(config =>
            {
                config.AddPolicy(Constants.IsBotOwner, policy => policy.RequireClaim(Constants.IsBotOwner, "true"));
            });

            await builder.Build().RunAsync();
        }
    }
}
