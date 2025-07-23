using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyBanHangDienTu.WebApp.Models.ViewModels;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Services.Location;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        private readonly ILocationService _locationService;

        public CartController(DataContext context, UserManager<UserModel> userManager, ILocationService locationService)
        {
            _dataContext = context;
            _userManager = userManager;
            _locationService = locationService;
        }
     
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            var cart = await _dataContext.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                Cart = cart,
                TotalAmount = cart.Sum(x => x.Quantity * x.Price),
                FullName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                userInformation = new UserInformationViewModel
                {
                    Address = user?.Address ?? "",
                    City = user != null ? await _locationService.GetCityNameById(user.City!) : "",
                    District = user != null ? await _locationService.GetDistrictNameById(user.City!, user.District!) : "",
                    Ward = user != null ? await _locationService.GetWardNameById(user.District!, user.Ward!) : ""
                }
            };

            var couponCode = HttpContext.Session.GetString("AppliedCoupon");
            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _dataContext.Coupons.FirstOrDefaultAsync(c =>
                    c.CouponCode == couponCode &&
                    c.Status == 1 &&
                    c.Quantity > 0 &&
                    c.StartDate <= DateTime.Now &&
                    c.EndDate >= DateTime.Now);

                if (coupon != null)
                {
                    var discount = coupon.DiscountType == DiscountType.Percent
                        ? (viewModel.TotalAmount * coupon.DiscountValue) / 100
                        : coupon.DiscountValue;

                    viewModel.CouponCode = couponCode;
                    viewModel.DiscountAmount = discount;

                    HttpContext.Session.SetString("DiscountAmount", discount.ToString());
                }
                else
                {
                    HttpContext.Session.Remove("AppliedCoupon");
                    HttpContext.Session.Remove("DiscountAmount");
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Add(int id)
        {
            var userId = _userManager.GetUserId(User);
            var userName = _userManager.GetUserName(User);
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null) return NotFound();

            var existingItem = await _dataContext.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var cartItem = new CartModel(product, userId!, userName!);
                _dataContext.Carts.Add(cartItem);
            }

            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Add to cart successfully.";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Increase(int id)
        {
            var userId = _userManager.GetUserId(User);
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null) return NotFound();

            var item = await _dataContext.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == id);

            if (item != null)
            {
                if (item.Quantity < product.Quantity)
                    item.Quantity++;
                else
                    TempData["error"] = "Maximum quantity reached.";
            }

            await _dataContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Decrease(int id)
        {
            var userId = _userManager.GetUserId(User);

            var item = await _dataContext.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == id);

            if (item != null)
            {
                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    _dataContext.Carts.Remove(item);
            }

            await _dataContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var item = await _dataContext.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == id);

            if (item != null)
            {
                _dataContext.Carts.Remove(item);
                await _dataContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string CouponCode)
        {
            if (string.IsNullOrWhiteSpace(CouponCode))
            {
                TempData["error"] = "Please enter coupon code.";
                return RedirectToAction("Index");
            }

            var coupon = await _dataContext.Coupons.FirstOrDefaultAsync(c =>
                c.CouponCode == CouponCode &&
                c.Status == 1 &&
                c.Quantity > 0 &&
                c.StartDate <= DateTime.Now &&
                c.EndDate >= DateTime.Now);

            if (coupon == null)
            {
                TempData["error"] = "Coupon code is invalid or expired.";
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);
            var cart = await _dataContext.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var total = cart.Sum(x => x.Quantity * x.Price);
            var discount = coupon.DiscountType == DiscountType.Percent
                ? (total * coupon.DiscountValue) / 100
                : coupon.DiscountValue;

            HttpContext.Session.SetString("AppliedCoupon", CouponCode);
            HttpContext.Session.SetString("DiscountAmount", discount.ToString());

            TempData["success"] = "Coupon code has been applied successfully.";
            return RedirectToAction("Index");
        }
    }
}
