using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyBanHangDienTu.WebApp.Models.MoMo;
using QuanLyBanHangDienTu.WebApp.Models.ViewModels;
using QuanLyBanHangDienTu.WebApp.Models.VNPay;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Services.EmailTemplates;
using QuanLyBanHangDienTu.WebApp.Services.Location;
using QuanLyBanHangDienTu.WebApp.Services.MoMo;
using QuanLyBanHangDienTu.WebApp.Services.VNPay;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly IMoMoService _moMoService;
        private readonly IVNPayService _vnPayService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailTemplateRenderer _emailRenderer;
        private readonly UserManager<UserModel> _userManager;
        private readonly ILocationService _locationService;

        public CheckoutController(DataContext context, IEmailSender emailSender, IMoMoService moMoService, IVNPayService vnPayService,
            IWebHostEnvironment webHostEnvironment, EmailTemplateRenderer emailTemplateRenderer,
            UserManager<UserModel> userManager, ILocationService locationService)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _moMoService = moMoService;
            _vnPayService = vnPayService;
            _webHostEnvironment = webHostEnvironment;
            _emailRenderer = emailTemplateRenderer;
            _userManager = userManager;
            _locationService = locationService;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userId = _userManager.GetUserId(User);
            if (userEmail == null || userId == null)
                return RedirectToAction("Login", "Account");

            var cart = await _dataContext.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var orderCode = Guid.NewGuid().ToString();
            int? couponId = null;
            var couponCode = HttpContext.Session.GetString("AppliedCoupon");
            var discountAmount = decimal.TryParse(HttpContext.Session.GetString("DiscountAmount"), out var parsedDiscount) ? parsedDiscount : 0;

            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _dataContext.Coupons.FirstOrDefaultAsync(c =>
                    c.CouponCode == couponCode && c.Status == 1 &&
                    c.Quantity > 0 && DateTime.Now >= c.StartDate && DateTime.Now <= c.EndDate);

                if (coupon != null)
                {
                    couponId = coupon.Id;
                    coupon.Quantity--;
                }
            }

            var user = await _userManager.FindByIdAsync(userId);

            var orderItem = new OrderModel
            {
                OrderCode = orderCode,
                UserName = userName!,
                PaymentMethod = PaymentMethod == "COD" ? "COD" : $"{PaymentMethod} {PaymentId}",
                Status = 1,
                CouponId = couponId,
                CouponCode = couponCode,

                FullName = user?.FullName ?? userName!,
                Email = user?.Email ?? userEmail,
                PhoneNumber = user?.PhoneNumber ?? "",
                Address = user?.Address ?? "",
                City = await _locationService.GetCityNameById(user?.City ?? ""),
                District = await _locationService.GetDistrictNameById(user?.City ?? "", user?.District ?? ""),
                Ward = await _locationService.GetWardNameById(user?.District ?? "", user?.Ward ?? "")
            };

            _dataContext.Orders.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            var orderDetails = new List<OrderDetailModel>();
            var emailItems = new List<EmailOrderItemViewModel>();
            decimal totalAmount = 0;

            foreach (var item in cart)
            {
                var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null || product.Quantity < item.Quantity)
                {
                    TempData["error"] = $"Product '{item.ProductName}' does not have sufficient quantity.";
                    return RedirectToAction("Index", "Cart");
                }

                product.Quantity -= item.Quantity;
                product.Sold += item.Quantity;

                orderDetails.Add(new OrderDetailModel
                {
                    OrderId = orderItem.Id,
                    OrderCode = orderCode,
                    UserName = userName!,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });

                emailItems.Add(new EmailOrderItemViewModel
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price
                });

                totalAmount += item.Price * item.Quantity;
            }

            _dataContext.OrderDetails.AddRange(orderDetails);
            await _dataContext.SaveChangesAsync(); // Lưu OrderDetail + Product update

            // Xoá giỏ hàng
            _dataContext.Carts.RemoveRange(cart);
            await _dataContext.SaveChangesAsync();

            // Xoá mã giảm giá đã dùng
            HttpContext.Session.Remove("AppliedCoupon");
            HttpContext.Session.Remove("DiscountAmount");

            // Gửi email xác nhận
            await SendEmailOrder(userEmail, userName!, orderCode, emailItems, totalAmount, couponCode, discountAmount);

            TempData["success"] = "Order successful!";
            return RedirectToAction("Index", "Home");
        }

        private async Task SendEmailOrder(string userEmail, string userName, string orderCode, List<EmailOrderItemViewModel> items,
            decimal totalAmount, string? couponCode, decimal discountAmount)
        {
            var viewModel = new EmailOrderViewModel
            {
                OrderCode = orderCode,
                UserName = userName,
                Items = items,
                TotalAmount = totalAmount,
                CouponCode = couponCode,
                DiscountAmount = discountAmount
            };

            var customerHtml = await _emailRenderer.RenderAsync("CustomerEmail.cshtml", viewModel);
            await _emailSender.SendEmailAsync(userEmail, "Xác Nhận Đơn Hàng", customerHtml);

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                var adminHtml = await _emailRenderer.RenderAsync("AdminEmail.cshtml", viewModel);
                await _emailSender.SendEmailAsync(admin.Email!, "Đơn Hàng Mới", adminHtml);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallBackMoMo()
        {
            var query = HttpContext.Request.Query;
            var resultCode = query["resultCode"];
            var orderId = query["orderId"];
            var orderInfo = query["orderInfo"];
            var amount = decimal.Parse(query["amount"]!);

            _ = _moMoService.PaymentExecuteAsync(query);

            if (resultCode != "0")
            {
                var momoModel = new MoMoModel
                {
                    OrderId = orderId,
                    OrderInfo = orderInfo,
                    Amount = amount,
                };

                _dataContext.Add(momoModel);
                await _dataContext.SaveChangesAsync();

                await Checkout("MoMo", orderId!);

                return View(new MoMoInformationModel
                {
                    OrderId = orderId,
                    OrderInfo = orderInfo,
                    Amount = (double)amount,
                    CreatedDate = momoModel.CreatedDate
                });
            }

            TempData["error"] = "Payment by MoMo failed.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallBackVNPay()
        {
            var response = await _vnPayService.PaymentExecuteAsync(HttpContext.Request.Query);

            if (response.VnPayResponseCode == "00")
            {
                var vnPayModel = new VNPayModel
                {
                    OrderId = response.OrderId,
                    OrderInfo = response.OrderInfo,
                    Amount = response.Amount,
                };

                _dataContext.Add(vnPayModel);
                await _dataContext.SaveChangesAsync();

                await Checkout(response.PaymentMethod!, response.OrderId!);

                return View(new VNPayInformationModel
                {
                    OrderId = response.OrderId,
                    OrderInfo = response.OrderInfo,
                    Amount = response.Amount,
                    CreatedDate = vnPayModel.CreatedDate
                });
            }

            TempData["error"] = "Payment by VNPay failed.";
            return RedirectToAction("Index", "Home");
        }
    }
}
