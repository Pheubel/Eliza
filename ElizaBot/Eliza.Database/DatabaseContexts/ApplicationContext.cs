using Eliza.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Eliza.Database.DatabaseContexts
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext()
        {
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<UserSubcribedTag> UserSubscribedTags { get; set; }
        public DbSet<UserBlacklistedTag> UserBlacklistedTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSubcribedTag>(entity =>
            {
                entity.HasKey(ut => new { ut.UserId, ut.TagId });

                entity.HasOne(ut => ut.Tag).WithMany(t => t.Subscribers).HasForeignKey(t => t.TagId);
                entity.HasOne(ut => ut.User).WithMany(t => t.SubscribedTags).HasForeignKey(t => t.UserId);
            });

            modelBuilder.Entity<UserBlacklistedTag>(entity =>
            {
                entity.HasKey(ut => new { ut.UserId, ut.TagId });

                entity.HasOne(ut => ut.Tag).WithMany(t => t.Blacklisters).HasForeignKey(t => t.TagId);
                entity.HasOne(ut => ut.User).WithMany(t => t.BlacklistedTags).HasForeignKey(t => t.UserId);
            });


            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasIndex(tag => tag.TagName).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
