using Microsoft.EntityFrameworkCore;
using btlAspNetCore.Models.DataModels;
using static NuGet.Packaging.PackagingConstants;

namespace btlAspNetCore.Models.DataModels
{
    public class AppDbContext:DbContext
    {
        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Banner> Banners { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }

        // Configure entity properties using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure BlogContent as TEXT
            modelBuilder.Entity<Blog>()
                .Property(b => b.BlogContent)
                .HasColumnType("text");

            modelBuilder.Entity<Product>()
           .Property(b => b.Description)
           .HasColumnType("text");

            // Category - Product (One-to-Many)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict) ;

            // Product - Rating (One-to-Many)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.ProductId);

            // User - Rating (One-to-Many)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId);

            // Product - ImgProduct (One-to-Many)
            modelBuilder.Entity<ImgProduct>()
                .HasOne(ip => ip.Product)
                .WithMany(p => p.ImgProducts)
                .HasForeignKey(ip => ip.ProductId);

            // User - Wishlist (One-to-Many)
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId);

            // Product - Wishlist (One-to-Many)
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId);

            // User - Order (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            // Order - OrderDetail (One-to-Many)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            // Product - OrderDetail (One-to-Many)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId);

            modelBuilder.Entity<OrderDetail>()
            .HasKey(od => new { od.OrderId, od.ProductId });
            modelBuilder.Entity<Rating>()
            .HasKey(r => new { r.ProductId, r.UserId });

            modelBuilder.Entity<Category>()
           .HasIndex(c => c.Name)
           .IsUnique();

            modelBuilder.Entity<Product>()
           .HasIndex(c => c.Name)
           .IsUnique();

            modelBuilder.Entity<User>()
           .HasIndex(u=>u.Email)
           .IsUnique();

        }

        // Configure entity properties using Fluent API
        public DbSet<btlAspNetCore.Models.DataModels.Banner>? Banner { get; set; }
    }
}
