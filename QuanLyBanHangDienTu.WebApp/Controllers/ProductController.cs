using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Models.ViewModels;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public ProductController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            var products = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToListAsync();

            ViewBag.Sliders = await _dataContext.Sliders
               .Where(s => s.Status == 1)
               .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _dataContext.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id);


            if (product == null) return NotFound();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                PhoneDetail = product.CategoryId == 1
                    ? await _dataContext.ProductDetailPhones.FirstOrDefaultAsync(x => x.ProductId == id)
                    : null,
                LaptopDetail = product.CategoryId == 2
                    ? await _dataContext.ProductDetailLaptops.FirstOrDefaultAsync(x => x.ProductId == id)
                    : null,
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index", "Home");
            }

            var products = await _dataContext.Products
                .Where(p => p.Name!.Contains(searchTerm))
                .ToListAsync();

            ViewBag.Sliders = await _dataContext.Sliders
               .Where(s => s.Status == 1)
               .ToListAsync();

            ViewBag.Keyword = searchTerm;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> SearchByPrice(decimal minPrice, decimal maxPrice)
        {
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            var products = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToListAsync();

            ViewBag.Sliders = await _dataContext.Sliders
            .Where(s => s.Status == 1)
            .ToListAsync();

            return View("Search", products);

        }
    }
}
