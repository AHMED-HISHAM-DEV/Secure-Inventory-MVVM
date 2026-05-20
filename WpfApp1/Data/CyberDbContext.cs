
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;
using WpfApp1.Models;  
namespace WpfApp1.Data
{
    public class CyberDbContext : DbContext   //using efcore
    {
        //using efcore
        // for understand this class, create this tables in database
        public DbSet<Product> Products { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseStock> WarehouseStocks { get; set; }
        public CyberDbContext()
        {
            // for check if Db created or not
            Database.EnsureCreated();
        }

        // for selected path of database
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CyberStockDB.sqlite");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        // ashaan aadel fee eltables elly 3ayez a3melhom configuration zay elkey w elrelationship w eldata seeding
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasConversion<double>();

            modelBuilder.Entity<Product>()
                .Property(p => p.CostPrice)
                .HasConversion<double>();

            // da data seeding lel user el admin
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "root", PasswordHash = hashedPassword, Role = UserRole.Admin }
            );

            //da Composite Key
            modelBuilder.Entity<WarehouseStock>()
                .HasKey(ws => new { ws.WarehouseId, ws.ProductId });


        }
    }
}