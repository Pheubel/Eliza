using Eliza.Client.Services;
using Eliza.Client.Services.Core;
using Eliza.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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
            builder.Services.AddScoped<IRoleApi, RoleApi>();
            builder.Services.AddScoped<ITagApi, TagApi>();

            builder.Services.AddAuthorizationCore(config =>
            {
                config.AddPolicy(Constants.IsBotOwner, policy => policy.RequireClaim(Constants.IsBotOwner, "true"));
            });

            await builder.Build().RunAsync();
        }
    }
}
