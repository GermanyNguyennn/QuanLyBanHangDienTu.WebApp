using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyBanHangDienTu.WebApp.Models.ViewModels;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Repository.Validation;
using QuanLyBanHangDienTu.WebApp.Services.Location;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        private readonly ILocationService _locationService;

        public OrderController(DataContext context, UserManager<UserModel> userManager, ILocationService locationService)
        {
            _dataContext = context;
            _userManager = userManager;
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            int count = await _dataContext.Orders.CountAsync();
            var pager = new Paginate(count, page, pageSize);

            var orders = await _dataContext.Orders
                .OrderByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return BadRequest("Invalid order code.");

            var order = await _dataContext.Orders
                .Include(o => o.Coupon)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
                return NotFound("Order not found.");

            var orderDetails = await _dataContext.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderCode == orderCode)
                .ToListAsync();

            ViewBag.Status = order.Status;
            ViewBag.OrderCode = orderCode;
            ViewBag.DiscountValue = order.Coupon?.DiscountValue ?? 0;
            ViewBag.DiscountType = order.Coupon?.DiscountType.ToString();

            return View(orderDetails);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateViewOrder(string orderCode, int status)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return BadRequest(new { success = false, message = "Missing order code." });

            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order == null)
                return BadRequest(new { success = false, message = "Order not found." });

            order.Status = status;

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Status updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error while updating status.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentMoMoOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest("Missing order code.");

            var moMo = await _dataContext.MoMos.FirstOrDefaultAsync(x => x.OrderId == orderId);
            if (moMo == null)
                return BadRequest("MoMo payment information not found.");

            return View(moMo);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentVNPayOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest("Missing order code.");

            var vnPay = await _dataContext.VNPays.FirstOrDefaultAsync(x => x.OrderId == orderId);
            if (vnPay == null)
                return BadRequest("VNPay payment information not found.");

            return View(vnPay);
        }

        [HttpGet]
        public async Task<IActionResult> UserOrder(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
                return NotFound("Missing order code.");

            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order == null)
                return NotFound("Order not found.");

            var viewModel = new UserInformationViewModel
            {
                FullName = order.FullName,
                Email = order.Email!,
                PhoneNumber = order.PhoneNumber!,               
                Address = order.Address,
                Ward = order.Ward,
                District = order.District,                
                City = order.City
            };

            return View(viewModel);
        }
    }
}
