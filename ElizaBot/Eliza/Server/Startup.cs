using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Eliza.Database.DatabaseContexts;
using Microsoft.EntityFrameworkCore;
using Discord;
using Eliza.Bot;
using Discord.WebSocket;
using Eliza.Database.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Security.Claims;
using Eliza.Shared;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Eliza.Bot.Services;

namespace Eliza.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var botSettings = Configuration.GetSection(nameof(BotSettings)).Get<BotSettings>();
            var authenticationSettigns = Configuration.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>();

            services.AddSingleton(botSettings);

            services.AddSingleton(Utilities.CreateDicordWebsocketClient(botSettings));
            services.AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>());

            services.AddSingleton(Utilities.CreateCommandService(botSettings));
            services.AddHostedService<BotService>();

            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Eliza.Server"));
            });

            MigrateDatabase(Configuration.GetConnectionString("ConnectionString"));

            services.AddScoped<TagService>();
            services.AddScoped<RequestableRoleManager>();
            services.AddScoped<IRoleService,RoleService>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie(options =>
                {
                    options.LoginPath = "/signin";
                    options.ExpireTimeSpan = new System.TimeSpan(7, 0, 0, 0);
                })
                .AddDiscord(options =>
                {
                    options.ClientId = authenticationSettigns.Discord.ClientId;
                    options.ClientSecret = authenticationSettigns.Discord.ClientSecret;

                    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                    {
                        OnCreatingTicket = ticketContext =>
                        {
                            if(ulong.TryParse(ticketContext.Principal.FindFirstValue(ClaimTypes.NameIdentifier),out var userId))
                            {
                                if(userId == botSettings.OwnerId)
                                {
                                    ticketContext.Principal.AddIdentity(new ClaimsIdentity(new[] { new Claim(Constants.IsBotOwner, "true")}));
                                }
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.IsBotOwner, policy => policy.RequireClaim(Constants.IsBotOwner, "true"));
            });

            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseClientRateLimiting();
            app.UseIpRateLimiting();

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private void MigrateDatabase(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<ApplicationContext>();
            builder.UseSqlite(connectionString, b => b.MigrationsAssembly("Eliza.Server"));

            using var context = new ApplicationContext(builder.Options);
            context.Database.Migrate();
        }
    }
}
