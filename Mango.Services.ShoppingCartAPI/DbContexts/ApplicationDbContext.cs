
using Mango.Services.ShoppingCartAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        //Create a table named Products which will store the Product Model
        public DbSet<Product> Products { get; set; }
        public DbSet<CartHeader> CartHeaders { get; set; }
        public DbSet<CartDetails> CartDetails { get; set; }
        //public DbSet<Cart> Cart{ get; set; }
        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartHeader>()
                .HasOne(c => c.CartDetails)
                .WithOne(c => c.CartHeader)
                .HasForeignKey<CartDetails>(c => c.CartHeaderId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.CartDetails)
                .WithOne(c => c.Product)
                .HasForeignKey<CartDetails>(c => c.ProductId);

        }
        */
    }
}
