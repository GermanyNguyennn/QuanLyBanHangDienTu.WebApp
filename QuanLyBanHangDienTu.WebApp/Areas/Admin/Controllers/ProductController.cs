using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBanHangDienTu.WebApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(1, page);
            var count = await _dataContext.Products.CountAsync();

            var pager = new Paginate(count, page, pageSize);
            var data = await _dataContext.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            return View(data);
        }

        [HttpGet]
        public IActionResult Add()
        {
            SetSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductModel productModel)
        {
            SetSelectLists(productModel.CategoryId, productModel.BrandId);

            productModel.Slug = GenerateSlug(productModel.Name!);

            if (await _dataContext.Products.AnyAsync(p => p.Slug == productModel.Slug))
            {
                TempData["error"] = "The product already exists.";
                return View(productModel);
            }

            if (productModel.ImageUpload != null)
                productModel.Image = await SaveImage(productModel.ImageUpload);

            _dataContext.Add(productModel);

            await _dataContext.SaveChangesAsync();           

            TempData["success"] = "Product added successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null) return NotFound();

            SetSelectLists(product.CategoryId, product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductModel productModel)
        {
            SetSelectLists(productModel.CategoryId, productModel.BrandId);

            var existingProduct = await _dataContext.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Slug = productModel.Name!.Trim().Replace(" ", "-");

            if (productModel.ImageUpload != null)
            {
                try
                {
                    DeleteImage(existingProduct.Image!);
                    existingProduct.Image = await SaveImage(productModel.ImageUpload);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to update image: " + ex.Message);
                    return View(productModel);
                }
            }

            existingProduct.Name = productModel.Name;
            existingProduct.Description = productModel.Description;
            existingProduct.Color = productModel.Color;
            existingProduct.Version = productModel.Version;
            existingProduct.Price = productModel.Price;
            existingProduct.ImportPrice = productModel.ImportPrice;
            existingProduct.Quantity = productModel.Quantity;
            existingProduct.CategoryId = productModel.CategoryId;
            existingProduct.BrandId = productModel.BrandId;

            _dataContext.Update(existingProduct);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Product updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null) return NotFound();

            DeleteImage(product.Image!);

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }       

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction("Index");

            var products = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Name!.Contains(searchTerm))
                .ToListAsync();

            ViewBag.Keyword = searchTerm;
            return View(products);
        }      

        private void SetSelectLists(int? categoryId = null, int? brandId = null)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", categoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", brandId);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            var fileName = Guid.NewGuid() + "_" + Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return fileName;
        }

        private void DeleteImage(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)) return;

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "media/products", imageName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _dataContext.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            if (product.CategoryId == 1) // Phone
            {
                var detail = await _dataContext.ProductDetailPhones
                    .FirstOrDefaultAsync(x => x.ProductId == id) ?? new ProductDetailPhoneModel
                    {
                        ProductId = product.Id,
                        CategoryId = product.CategoryId,
                        BrandId = product.BrandId,
                    };

                return View("ViewDetailPhone", detail);
            }
            else if (product.CategoryId == 2) // Laptop
            {
                var detail = await _dataContext.ProductDetailLaptops
                    .FirstOrDefaultAsync(x => x.ProductId == id) ?? new ProductDetailLaptopModel
                    {
                        ProductId = product.Id,
                        CategoryId = product.CategoryId,
                        BrandId = product.BrandId,
                    };

                return View("ViewDetailLaptop", detail);
            }

            return NotFound("Category does not support displaying details.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDetail(IFormCollection form)
        {
            int categoryId = int.Parse(form["CategoryId"]!);

            int productId = int.Parse(form["ProductId"]!);

            if (categoryId == 1)
            {
                var detail = await _dataContext.ProductDetailPhones
                    .FirstOrDefaultAsync(x => x.ProductId == productId);

                if (detail == null)
                {
                    detail = new ProductDetailPhoneModel();
                    _dataContext.ProductDetailPhones.Add(detail);
                }

                detail.ProductId = productId;
                detail.CategoryId = categoryId;
                detail.BrandId = int.Parse(form["BrandId"]!);
                detail.ScreenSize = form["ScreenSize"]!;
                detail.DisplayTechnology = form["DisplayTechnology"]!;
                detail.RearCamera = form["RearCamera"]!;
                detail.FrontCamera = form["FrontCamera"]!;
                detail.ChipSet = form["ChipSet"]!;
                detail.NFC = form["NFC"].ToString() == "true";
                detail.RAMCapacity = form["RAMCapacity"]!;
                detail.InternalStorage = form["InternalStorage"]!;
                detail.SimCard = form["SimCard"]!;
                detail.OperatingSystem = form["OperatingSystem"]!;
                detail.DisplayResolution = form["DisplayResolution"]!;
                detail.DisplayFeatures = form["DisplayFeatures"]!;
                detail.CPUType = form["CPUType"]!;

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Phone details saved successfully!";
            }
            else if (categoryId == 2) 
            {
                var detail = await _dataContext.ProductDetailLaptops
                    .FirstOrDefaultAsync(x => x.ProductId == productId);

                if (detail == null)
                {
                    detail = new ProductDetailLaptopModel();
                    _dataContext.ProductDetailLaptops.Add(detail);
                }

                detail.ProductId = productId;
                detail.CategoryId = categoryId;
                detail.BrandId = int.Parse(form["BrandId"]!);
                detail.GraphicsCardType = form["GraphicsCardType"]!;
                detail.RAMCapacity = form["RAMCapacity"]!;
                detail.RAMType = form["RAMType"]!;
                detail.NumberOfRAMSlots = form["NumberOfRAMSlots"]!;
                detail.HardDrive = form["HardDrive"]!;
                detail.ScreenSize = form["ScreenSize"]!;
                detail.ScreenTechnology = form["ScreenTechnology"]!;
                detail.Battery = form["Battery"]!;
                detail.OperatingSystem = form["OperatingSystem"]!;
                detail.ScreenResolution = form["ScreenResolution"]!;
                detail.CPUType = form["CPUType"]!;
                detail.Interface = form["Interface"]!;

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Laptop details saved successfully!";
            }
            else
            {
                return BadRequest("Invalid category.");
            }

            return RedirectToAction("Index");
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            // Bước 0: Thay thế các ký tự đặc biệt tiếng Việt như Đ/đ
            name = name.Replace("Đ", "D").Replace("đ", "d");

            // Bước 1: Chuẩn hóa Unicode (loại bỏ dấu tiếng Việt)
            string normalized = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            string slug = sb.ToString().Normalize(NormalizationForm.FormC);

            // Bước 2: Chuyển sang chữ thường và loại bỏ ký tự đặc biệt
            slug = slug.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");      // chỉ giữ lại chữ, số, khoảng trắng và -
            slug = Regex.Replace(slug, @"\s+", "-");              // thay khoảng trắng bằng dấu gạch ngang
            slug = Regex.Replace(slug, @"-+", "-");               // gộp nhiều dấu - liền nhau thành 1

            return slug.Trim('-'); // loại bỏ dấu - ở đầu/cuối
        }       
    }
}
