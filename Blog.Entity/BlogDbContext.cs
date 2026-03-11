using Blog.Entity.Entities;
using MicroBase.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Entity
{
    public class BlogDbContext : AppDbContext<BlogDbContext>
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Blog> Blogs { get; set; }

        public DbSet<BlogCategory> BlogCategories { get; set; }

        public DbSet<BlogCategoryMap> BlogCategoryMaps { get; set; }

        public DbSet<BlogTagMap> BlogTagMaps { get; set; }

        public DbSet<BlogTag> BlogTags { get; set; }

        public DbSet<LocalizationKey> LocalizationKey { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);

            modelBuilder.Ignore<IdentityRole<int>>();
            modelBuilder.Ignore<IdentityUser<int>>();
            modelBuilder.Ignore<IdentityUserRole<int>>();
            modelBuilder.Ignore<IdentityRoleClaim<int>>();
            modelBuilder.Ignore<IdentityUserLogin<int>>();
            modelBuilder.Ignore<IdentityUserToken<int>>();
        }
    }
}