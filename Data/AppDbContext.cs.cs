using Irish_Beauty_Product.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Irish_Beauty_Product.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entity -> table
            modelBuilder.Entity<User>().ToTable("User");
        }
    }
}  