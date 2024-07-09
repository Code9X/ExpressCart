using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ExpressCart.Models;

namespace ExpressCart.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<CategoryImage> CategoryImages { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
		public DbSet<Addvertisement> Addvertisements { get; set; }
		public DbSet<AddvertisementImage> AddvertisementImages { get; set; }
		public DbSet<Travel> Travels { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData
            (
                new Category { Id = 1, Name = "Electronics", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Fashion", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Home & Furniture", DisplayOrder = 3 },
                new Category { Id = 4, Name = "Travel", DisplayOrder = 4 },
                new Category { Id = 5, Name = "Beauty, Toys & More", DisplayOrder = 5 }
            );
        }

    }
}
