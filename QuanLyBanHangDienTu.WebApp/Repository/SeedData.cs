using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Models;

namespace QuanLyBanHangDienTu.WebApp.Repository
{
    public class SeedData
    {
        public static async Task SeedingDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<DataContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserModel>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.Migrate();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            var adminEmail = "manhducnguyen23092003@gmail.com";
            var adminPhone = "0964429403";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new UserModel
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    PhoneNumber = adminPhone,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "23092003");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (!context.Brands.Any())
            {
                // Seed Brands
                var iphone = new BrandModel { Name = "Iphone", Description = "", Slug = "iphone", Status = 1 };
                var mac = new BrandModel { Name = "Mac", Description = "", Slug = "mac", Status = 1 };
                var windows = new BrandModel { Name = "Windows", Description = "", Slug = "windows", Status = 1 };
                await context.Brands.AddRangeAsync(iphone, mac, windows);
                await context.SaveChangesAsync(); // ✅ Đảm bảo Id được sinh
              
            }

            if (!context.Categories.Any())
            {
                // Seed Categories
                var phone = new CategoryModel { Name = "Phone", Description = "", Slug = "phone", Status = 1 };
                var laptop = new CategoryModel { Name = "Laptop", Description = "", Slug = "laptop", Status = 1 };
                await context.Categories.AddRangeAsync(phone, laptop);
                await context.SaveChangesAsync(); // ✅ Đảm bảo Id được sinh              
            }

            if (!context.Products.Any())
            {
                // Lấy Brand và Category đã seed
                var iphoneBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "iphone");
                var macBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "mac");
                var windowsBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "windows");

                var phoneCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "phone");
                var laptopCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "laptop");

                if (iphoneBrand != null && macBrand != null && windowsBrand != null &&
                    phoneCategory != null && laptopCategory != null)
                {
                    var products = new List<ProductModel>
                    {
                        new ProductModel
                        {
                            Name = "iPhone 15 Pro Max 256GB",
                            Image = "iphone-16.jpg",
                            Description = "iPhone 15 Pro Max với chip A17 Pro mạnh mẽ, camera 48MP và thiết kế titan sang trọng.",
                            Color = "Titan Xanh",
                            Version = "256GB",
                            Price = 34990000,
                            ImportPrice = 28000000,
                            Quantity = 50,
                            Sold = 10,
                            Slug = "iphone-16",
                            BrandId = iphoneBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "iPhone 14 Pro 128GB",
                            Image = "iphone-16.jpg",
                            Description = "iPhone 14 Pro với chip A16 Bionic, Dynamic Island, và camera chuyên nghiệp.",
                            Color = "Tím",
                            Version = "128GB",
                            Price = 26990000,
                            ImportPrice = 22000000,
                            Quantity = 60,
                            Sold = 15,
                            Slug = "iphone-16e",
                            BrandId = iphoneBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "iPhone 13 128GB",
                            Image = "iphone-16.jpg",
                            Description = "iPhone 13 với chip A15 Bionic, hiệu năng mạnh mẽ và thời lượng pin dài.",
                            Color = "Đỏ",
                            Version = "128GB",
                            Price = 18990000,
                            ImportPrice = 16000000,
                            Quantity = 70,
                            Sold = 20,
                            Slug = "iphone-16-plus",
                            BrandId = iphoneBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "MacBook Air M2 13 inch 2024",
                            Image = "iphone-16.jpg",
                            Description = "MacBook Air M2 mỏng nhẹ, hiệu năng vượt trội, pin lên tới 18 giờ.",
                            Color = "Bạc",
                            Version = "8GB/256GB",
                            Price = 28990000,
                            ImportPrice = 24000000,
                            Quantity = 30,
                            Sold = 5,
                            Slug = "macbook-air",
                            BrandId = macBrand.Id,
                            CategoryId = laptopCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "MacBook Pro 14 inch M3 2024",
                            Image = "iphone-16.jpg",
                            Description = "MacBook Pro M3 cho hiệu năng đột phá, màn mini-LED tuyệt đẹp và pin 20 giờ.",
                            Color = "Xám không gian",
                            Version = "16GB/512GB",
                            Price = 49990000,
                            ImportPrice = 43000000,
                            Quantity = 25,
                            Sold = 6,
                            Slug = "macbook-pro",
                            BrandId = macBrand.Id,
                            CategoryId = laptopCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "Surface Laptop 5 13.5 inch",
                            Image = "iphone-16.jpg",
                            Description = "Surface Laptop 5 mang đến sự kết hợp hoàn hảo giữa hiệu năng và phong cách.",
                            Color = "Bạch Kim",
                            Version = "Core i5 / 8GB / 512GB",
                            Price = 25990000,
                            ImportPrice = 21000000,
                            Quantity = 25,
                            Sold = 8,
                            Slug = "surface-laptop-5",
                            BrandId = windowsBrand.Id,
                            CategoryId = laptopCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "Surface Pro 9 13 inch",
                            Image = "iphone-16.jpg",
                            Description = "Surface Pro 9 mang đến trải nghiệm linh hoạt giữa laptop và tablet, hiệu năng cao.",
                            Color = "Xanh Dương",
                            Version = "Core i7 / 16GB / 512GB",
                            Price = 37990000,
                            ImportPrice = 32000000,
                            Quantity = 20,
                            Sold = 4,
                            Slug = "surface-pro-9",
                            BrandId = windowsBrand.Id,
                            CategoryId = laptopCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "iPhone SE 2022 64GB",
                            Image = "iphone-16.jpg",
                            Description = "iPhone SE 2022 nhỏ gọn, chip A15 Bionic và cảm biến vân tay Touch ID truyền thống.",
                            Color = "Trắng",
                            Version = "64GB",
                            Price = 10990000,
                            ImportPrice = 9500000,
                            Quantity = 80,
                            Sold = 30,
                            Slug = "iphone-se-2022-64gb",
                            BrandId = iphoneBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "MacBook Air M1 2020",
                            Image = "iphone-16.jpg",
                            Description = "MacBook Air M1 mang đến hiệu năng tuyệt vời, hoạt động êm ái không quạt, giá hợp lý.",
                            Color = "Vàng",
                            Version = "8GB/256GB",
                            Price = 21990000,
                            ImportPrice = 18000000,
                            Quantity = 40,
                            Sold = 18,
                            Slug = "macbook-air-m1-2020",
                            BrandId = macBrand.Id,
                            CategoryId = laptopCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "Surface Laptop Go 2",
                            Image = "iphone-16.jpg",
                            Description = "Surface Laptop Go 2 nhỏ gọn, pin lâu, màn hình cảm ứng 12.4 inch.",
                            Color = "Xanh nhạt",
                            Version = "Core i5 / 8GB / 256GB",
                            Price = 18990000,
                            ImportPrice = 15000000,
                            Quantity = 35,
                            Sold = 12,
                            Slug = "surface-laptop-go-2",
                            BrandId = windowsBrand.Id,
                            CategoryId = laptopCategory.Id,
                            CreatedDate = DateTime.Now
                        }
                    };

                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                }
            }

            if (!context.Contacts.Any())
            {
                ContactModel contactModel = new ContactModel
                {
                    Name = "Nguyễn Mạnh Đức",
                    Map = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3723.883773346561!2d105.85400031440625!3d21.00500009317313!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3135ab4000000001%3A0x0000000000000001!2sNgõ%2084%20Phố%208%2F3%2C%20Quỳnh%20Mai%2C%20Hai%20Bà%20Trưng%2C%20Hà%20Nội!5e0!3m2!1svi!2s!4v1680000000000",
                    Email = "manhducnguyen23092003@gmail.com",
                    Phone = "0964429403",
                    Address = "Hà Nội",
                    Description = "",
                    LogoImage = "5c12ee81-ed3b-4d66-a529-5f93af1726ff_Admin.jpg"
                };

                context.Contacts.Add(contactModel);
                await context.SaveChangesAsync();
            }
        }
    }
}
