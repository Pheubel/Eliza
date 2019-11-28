using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ElizaBot.DatabaseContexts.DesignTimeFactories
{
    public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            var settings = new AppSettingsLoader().LoadSettings<AppSettings>();
            var builder = new DbContextOptionsBuilder<ApplicationContext>();
            builder.UseSqlite(settings.ConnectionString);

            return new ApplicationContext(builder.Options);
        }
    }
}
