using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ECommerceGestao.Models;

namespace ECommerceGestao.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);
                
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);
                
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);
                
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
                
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);

            // Seed Categories and Products
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Eletrônicos", Description = "Dispositivos e acessórios eletrônicos" },
                new Category { Id = 2, Name = "Vestuário", Description = "Roupas e acessórios" },
                new Category { Id = 3, Name = "Livros", Description = "Livros de diversos gêneros" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Smartphone X",
                    Description = "Smartphone de última geração com 128GB de memória e câmera de alta resolução",
                    Price = 1999.99m,
                    Stock = 50,
                    ImageUrl = "/images/smartphone.jpg",
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Camiseta Básica",
                    Description = "Camiseta 100% algodão em diversas cores",
                    Price = 39.90m,
                    Stock = 200,
                    ImageUrl = "/images/tshirt.jpg",
                    CategoryId = 2
                },
                new Product
                {
                    Id = 3,
                    Name = "Romance Bestseller",
                    Description = "Livro mais vendido do ano, história emocionante",
                    Price = 59.90m,
                    Stock = 30,
                    ImageUrl = "/images/book.jpg",
                    CategoryId = 3
                }
            );
        }
    }
}
