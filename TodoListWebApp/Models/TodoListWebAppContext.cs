using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoListWebApp.Services;

namespace TodoListWebApp.Models
{
    public class TodoListWebAppContext : DbContext
    {
        public DbSet<Todo> Todoes { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AADUserRecord> Users { get; set; }
        public DbSet<PerWebUserCache> PerUserCacheList { get; set; }

        public TodoListWebAppContext(DbContextOptions<TodoListWebAppContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
