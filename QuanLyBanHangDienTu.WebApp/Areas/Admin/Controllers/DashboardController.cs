using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBanHangDienTu.WebApp.Models.Statistical;
using QuanLyBanHangDienTu.WebApp.Models;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;

        public DashboardController(DataContext context)
        {
            _dataContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int? categoryId, int? brandId, string statisticType = "day")
        {
            await LoadOverviewCounters();
            await LoadFilterSelections(categoryId, brandId, statisticType);

            var statistics = await GetStatisticalData(fromDate, toDate, categoryId, brandId);
            var orders = await GetFilteredOrders(fromDate, toDate);

            return View(new StatisticalFilterViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Statistics = statistics
            });
        }

        private async Task LoadOverviewCounters()
        {
            ViewBag.CountProduct = await _dataContext.Products.CountAsync();
            ViewBag.CountOrder = await _dataContext.Orders.CountAsync();
            ViewBag.CountCategory = await _dataContext.Categories.CountAsync();
            ViewBag.CountUser = await _dataContext.Users.CountAsync();
        }

        private async Task LoadFilterSelections(int? categoryId, int? brandId, string statisticType)
        {
            ViewBag.Categories = await _dataContext.Categories.ToListAsync();
            ViewBag.Brands = await _dataContext.Brands.ToListAsync();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedBrand = brandId;
            ViewBag.SelectedStatisticType = statisticType;
        }

        private async Task<List<StatisticalViewModel>> GetStatisticalData(DateTime? fromDate, DateTime? toDate, int? categoryId, int? brandId)
        {
            var filteredOrderDetails = _dataContext.OrderDetails
                .Where(od =>
                    (!fromDate.HasValue || od.Order!.CreatedDate.Date >= fromDate.Value.Date) &&
                    (!toDate.HasValue || od.Order!.CreatedDate.Date <= toDate.Value.Date) &&
                    (!categoryId.HasValue || od.Product!.CategoryId == categoryId) &&
                    (!brandId.HasValue || od.Product!.BrandId == brandId)
                )
                .Select(od => new
                {
                    od.ProductId,
                    ProductName = od.Product!.Name,
                    ProductImage = od.Product.Image,
                    ProductImportPrice = od.Product.ImportPrice,

                    od.Price,
                    od.Quantity,

                    OrderId = od.Order!.Id,
                    OrderCreatedDate = od.Order.CreatedDate,
                    CouponId = od.Order.CouponId,
                    Coupon = od.Order.Coupon,

                    OrderTotal = od.Order.OrderDetail.Sum(x => x.Price * x.Quantity)
                });

            var list = await filteredOrderDetails
                .AsNoTracking()
                .ToListAsync();

            var grouped = list
                .GroupBy(x => new { x.ProductId, x.ProductName, x.ProductImage, x.ProductImportPrice })
                .Select(group =>
                {
                    var totalQuantity = group.Sum(x => x.Quantity);
                    var totalRevenue = group.Sum(x => x.Price * x.Quantity);
                    var totalCost = group.Sum(x => x.Quantity * x.ProductImportPrice);

                    var withCoupon = group.Where(x => x.CouponId != null).ToList();
                    var withoutCoupon = group.Where(x => x.CouponId == null).ToList();

                    var revenueWithCoupon = withCoupon.Sum(x => x.Price * x.Quantity);
                    var costWithCoupon = withCoupon.Sum(x => x.Quantity * x.ProductImportPrice);

                    var revenueWithoutCoupon = withoutCoupon.Sum(x => x.Price * x.Quantity);
                    var costWithoutCoupon = withoutCoupon.Sum(x => x.Quantity * x.ProductImportPrice);

                    var totalDiscountCoupon = withCoupon.Sum(x =>
                    {
                        var orderDetails = list.Where(o => o.OrderId == x.OrderId).ToList();
                        var orderTotal = orderDetails.Sum(od => od.Price * od.Quantity);
                        var thisLineTotal = x.Price * x.Quantity;

                        var discount = x.Coupon == null ? 0 :
                            (x.Coupon.DiscountType == DiscountType.Percent
                                ? orderTotal * x.Coupon.DiscountValue / 100
                                : x.Coupon.DiscountValue);

                        var allocatedDiscount = orderTotal > 0 ? (thisLineTotal / orderTotal) * discount : 0;
                        return Math.Min(allocatedDiscount, thisLineTotal);
                    });

                    return new StatisticalViewModel
                    {
                        ProductId = group.Key.ProductId,
                        ProductName = group.Key.ProductName,
                        Image = group.Key.ProductImage,

                        TotalQuantitySold = totalQuantity,
                        TotalRevenue = totalRevenue,
                        TotalCost = totalCost,

                        QuantityWithCoupon = withCoupon.Sum(x => x.Quantity),
                        QuantityWithoutCoupon = withoutCoupon.Sum(x => x.Quantity),

                        RevenueWithCoupon = revenueWithCoupon,
                        RevenueWithoutCoupon = revenueWithoutCoupon,

                        CostWithCoupon = costWithCoupon,
                        CostWithoutCoupon = costWithoutCoupon,

                        TotalDiscountCoupon = totalDiscountCoupon,

                        FirstSoldDate = group.Min(x => x.OrderCreatedDate),
                        LastSoldDate = group.Max(x => x.OrderCreatedDate)
                    };
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            return grouped;
        }

        private async Task<List<OrderModel>> GetFilteredOrders(DateTime? fromDate, DateTime? toDate)
        {
            return await _dataContext.Orders
                .Where(o =>
                    (!fromDate.HasValue || o.CreatedDate.ToLocalTime().Date >= fromDate.Value.Date) &&
                    (!toDate.HasValue || o.CreatedDate.ToLocalTime().Date <= toDate.Value.Date))
                .Include(o => o.OrderDetail)
                .Include(o => o.Coupon)
                .ToListAsync();
        }       
    }
}
