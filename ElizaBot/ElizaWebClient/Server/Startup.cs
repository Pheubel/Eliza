using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using ElizaBot;
using ElizaBot.Extensions;
using Microsoft.Extensions.Configuration;
using ElizaBot.DatabaseContexts;
using Microsoft.EntityFrameworkCore;

namespace ElizaWebClient.Server
{
    public class Startup
    {
        IConfiguration _configuration;
        IWebHostEnvironment _webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<BotConfig>(_configuration.GetSection(nameof(BotConfig)));

            services.AddMvc();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddDbContext<ApplicationContext>(options => {
                options.UseSqlite(_configuration.GetConnectionString("ConnectionString"));
            });

            MigrateDatabase(_configuration.GetConnectionString("ConnectionString"));

            services.AddEliza();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Startup>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });
        }

        private void MigrateDatabase(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<ApplicationContext>();
            builder.UseSqlite(connectionString);

            using var context = new ApplicationContext(builder.Options);
            context.Database.Migrate();
        }
    }
}
