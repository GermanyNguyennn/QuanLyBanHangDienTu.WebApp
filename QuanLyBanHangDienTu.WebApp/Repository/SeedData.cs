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
                var apple = new BrandModel { Name = "Apple", Description = "", Slug = "apple", Status = 1 };
                var samsung = new BrandModel { Name = "Samsung", Description = "", Slug = "samsung", Status = 1 };
                var asus = new BrandModel { Name = "Asus", Description = "", Slug = "asus", Status = 1 };
                await context.Brands.AddRangeAsync(apple, samsung, asus);
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
                var appleBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "apple");
                var samsungBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "samsung");
                var asusBrand = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "asus");

                var phoneCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "phone");
                var laptopCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "laptop");

                if (appleBrand != null && samsungBrand != null && asusBrand != null &&
                    phoneCategory != null && laptopCategory != null)
                {
                    var products = new List<ProductModel>
                    {
                        new ProductModel
                        {
                            Name = "iPhone 17 Pro Max 2TB Cam Vũ Trụ",
                            Image = "iphone-17-pro-max-2-tb-cam-vu-tru.webp",
                            Description = "iPhone 17 Pro Max 2TB Cam Vũ Trụ.",
                            Color = "Cam Vũ Trụ",
                            Version = "2TB",
                            Price = 63990000,
                            ImportPrice = 20000000,
                            Quantity = 50,
                            Sold = 10,
                            Slug = "iphone-17-pro-max-2-tb-cam-vu-tru",
                            BrandId = appleBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "iPhone 17 Pro Max 1TB Cam Vũ Trụ",
                            Image = "iphone-17-pro-max-1-tb-cam-vu-tru.webp",
                            Description = "iPhone 17 Pro Max 1TB Cam Vũ Trụ.",
                            Color = "Cam Vũ Trụ",
                            Version = "1TB",
                            Price = 50990000,
                            ImportPrice = 20000000,
                            Quantity = 50,
                            Sold = 10,
                            Slug = "iphone-17-pro-max-1-tb-cam-vu-tru",
                            BrandId = appleBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "iPhone 17 Pro Max 512GB Cam Vũ Trụ",
                            Image = "iphone-17-pro-max-512-gb-cam-vu-tru.webp",
                            Description = "iPhone 17 Pro Max 512GB Cam Vũ Trụ.",
                            Color = "Cam Vũ Trụ",
                            Version = "512GB",
                            Price = 43990000,
                            ImportPrice = 20000000,
                            Quantity = 50,
                            Sold = 10,
                            Slug = "iphone-17-pro-max-512-gb-cam-vu-tru",
                            BrandId = appleBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },
                        new ProductModel
                        {
                            Name = "iPhone 17 Pro Max 256GB Cam Vũ Trụ",
                            Image = "iphone-17-pro-max-256-gb-cam-vu-tru.webp",
                            Description = "iPhone 17 Pro Max 256GB Cam Vũ Trụ.",
                            Color = "Cam Vũ Trụ",
                            Version = "256GB",
                            Price = 37890000,
                            ImportPrice = 20000000,
                            Quantity = 50,
                            Sold = 10,
                            Slug = "iphone-17-pro-max-256-gb-cam-vu-tru",
                            BrandId = appleBrand.Id,
                            CategoryId = phoneCategory.Id,
                            CreatedDate = DateTime.Now
                        },                       
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
