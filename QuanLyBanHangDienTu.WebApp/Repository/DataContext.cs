using QuanLyBanHangDienTu.WebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuanLyBanHangDienTu.WebApp.Repository
{
    public class DataContext : IdentityDbContext<UserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ 1-1 giữa Product và ProductDetailPhone, ProductDetailLaptop, ProductDetailTablet
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.ProductDetailLaptops)
                .WithOne(d => d.Product)
                .HasForeignKey<ProductDetailLaptopModel>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.ProductDetailPhones)
                .WithOne(d => d.Product)
                .HasForeignKey<ProductDetailPhoneModel>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.ProductDetailTablets)
                .WithOne(d => d.Product)
                .HasForeignKey<ProductDetailTabletModel>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1-n giữa Product và OrderDetail
            modelBuilder.Entity<ProductModel>()
                .HasMany(p => p.OrderDetails)
                .WithOne(o => o.Product)
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tùy chỉnh nếu cần: Brand, Category không cần cascade
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Brand)
                .WithMany()
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        }

        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetailModel> OrderDetails { get; set; }
        public DbSet<SliderModel> Sliders { get; set; }
        public DbSet<ContactModel> Contacts { get; set; }
        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<MoMoModel> MoMos { get; set; }
        public DbSet<VNPayModel> VNPays { get; set; }
        public DbSet<ProductDetailPhoneModel> ProductDetailPhones { get; set; }
        public DbSet<ProductDetailLaptopModel> ProductDetailLaptops { get; set; }
        public DbSet<ProductDetailTabletModel> ProductDetailTablets { get; set; }
        public DbSet<CartModel> Carts { get; set; }
    }
}
