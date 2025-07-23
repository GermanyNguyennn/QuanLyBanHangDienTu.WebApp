using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Controllers
{
    public class CompanyController : Controller
    {
        private readonly DataContext _dataContext;
        public CompanyController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            var Company = await _dataContext.Categories
                .Where(c => c.Slug == Slug)
                .FirstOrDefaultAsync();

            if (Company == null)
                return RedirectToAction("Index", "Home");

            var query = _dataContext.Products
                .Where(p => p.CompanyId == Company.Id);

            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out decimal startPriceVal) &&
                    decimal.TryParse(endprice, out decimal endPriceVal))
                {
                    query = query.Where(p => p.Price >= startPriceVal && p.Price <= endPriceVal);
                }
            }

            switch (sort_by)
            {
                case "price_increase":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_decrease":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "price_newest":
                    query = query.OrderByDescending(p => p.Id);
                    break;
                case "price_oldest":
                    query = query.OrderBy(p => p.Id);
                    break;
                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            var productsByCompany = await query.ToListAsync();

            ViewBag.Sliders = await _dataContext.Sliders
                .Where(s => s.Status == 1)
                .ToListAsync();
            ViewBag.count = productsByCompany.Count;
            ViewBag.sort_key = sort_by;

            return View(productsByCompany);
        }
    }
}
