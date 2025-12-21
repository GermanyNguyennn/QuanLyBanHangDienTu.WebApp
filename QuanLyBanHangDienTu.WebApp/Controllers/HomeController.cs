using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Models.ViewModels;
using QuanLyBanHangDienTu.WebApp.Repository;
using System.Diagnostics;

namespace QuanLyBanHangDienTu.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _dataContext;

        public HomeController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
            _dataContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? sort_by)
        {
            var query = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            query = sort_by switch
            {
                "price_increase" => query.OrderBy(p => p.Price),
                "price_decrease" => query.OrderByDescending(p => p.Price),
                "price_newest" => query.OrderByDescending(p => p.CreatedDate),
                "price_oldest" => query.OrderBy(p => p.CreatedDate),
                _ => query
            };

            var products = await query.ToListAsync();

            ViewBag.Sliders = await _dataContext.Sliders
                .Where(s => s.Status == 1)
                .ToListAsync();

            ViewBag.SortBy = sort_by;

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
                TabletDetail = product.CategoryId == 3
                    ? await _dataContext.ProductDetailTablets.FirstOrDefaultAsync(x => x.ProductId == id)
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
        public async Task<IActionResult> SearchByPrice(decimal minPrice, decimal maxPrice, string? sort_by)
        {
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            var query = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .AsQueryable();

            query = sort_by switch
            {
                "price_increase" => query.OrderBy(p => p.Price),
                "price_decrease" => query.OrderByDescending(p => p.Price),
                "price_newest" => query.OrderByDescending(p => p.CreatedDate),
                "price_oldest" => query.OrderBy(p => p.CreatedDate),
                _ => query
            };

            var products = await query.ToListAsync();

            ViewBag.Sliders = await _dataContext.Sliders
                .Where(s => s.Status == 1)
                .ToListAsync();

            ViewBag.SortBy = sort_by;

            return View("Search", products);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
