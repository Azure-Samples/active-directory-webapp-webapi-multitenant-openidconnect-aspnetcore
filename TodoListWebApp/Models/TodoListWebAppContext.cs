using Microsoft.Data.Entity;
using TodoListWebApp.Services;

namespace TodoListWebApp.Models
{
    public class TodoListWebAppContext : DbContext
    {
        public DbSet<Todo> Todoes { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AADUserRecord> Users { get; set; }
        public DbSet<PerWebUserCache> PerUserCacheList { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
