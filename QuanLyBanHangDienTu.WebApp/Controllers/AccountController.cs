using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyBanHangDienTu.WebApp.Models.ViewModels;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Repository.Validation;
using System.Security.Claims;
using System.Web;
using QuanLyBanHangDienTu.WebApp.Services.EmailTemplates;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;


        public AccountController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IEmailSender emailSender, DataContext dataContext, RoleManager<IdentityRole> roleManager)
        {
            _emailSender = emailSender;
            _dataContext = dataContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        private async Task<UserModel?> GetUserFromContextAsync()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return await _userManager.GetUserAsync(User);

            if (TempData["UserId"] != null)
            {
                var userId = TempData["UserId"]!.ToString();
                TempData.Keep("UserId");
                return await _userManager.FindByIdAsync(userId!);
            }

            return null;
        }

        [HttpGet]
        public IActionResult Login(string returnURL)
        {
            return View(new LoginViewModel { ReturnURL = returnURL });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(model.UserName!);
            if (user == null)
            {
                TempData["error"] = "Account does not exist.";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password!, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {            
                TempData["success"] = "Log in successfully.";

                if (!string.IsNullOrEmpty(model.ReturnURL) && Url.IsLocalUrl(model.ReturnURL))
                    return Redirect(model.ReturnURL);

                return RedirectToAction("Index", "Home");
            }

            TempData["error"] = "Login failed. Check information again.";
            return View(model);
        }
        public async Task<IActionResult> Logout(string returnURL = "/")
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            TempData["success"] = "Log out successfully.";
            return Redirect(returnURL);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return View(registerViewModel);

            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                TempData["error"] = "The administrator has not created the 'Customer' role.";
                return View(registerViewModel);
            }

            var newUser = new UserModel
            {
                UserName = registerViewModel.UserName,
                Email = registerViewModel.Email,
                PhoneNumber = registerViewModel.PhoneNumber
            };

            var result = await _userManager.CreateAsync(newUser, registerViewModel.Password!);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(newUser, "Customer");
                if (roleResult.Succeeded)
                {
                    TempData["success"] = "Registration successful.";
                    return RedirectToAction("Login");
                }

                TempData["error"] = "Role assignment failed.";
                await _userManager.DeleteAsync(newUser);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(registerViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> HistoryOrder(int page = 1)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 10;
            if (page < 1) page = 1;

            var userName = User.FindFirstValue(ClaimTypes.Name);
            var totalOrders = await _dataContext.Orders
                .Where(o => o.UserName == userName)
                .CountAsync();

            var pager = new Paginate(totalOrders, page, pageSize);

            var orders = await _dataContext.Orders
                .Where(o => o.UserName == userName)
                .OrderByDescending(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            ViewBag.UserEmail = userName;

            return View(orders);
        }


        [HttpGet]
        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return NotFound();

            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
                return NotFound();

            var orderDetails = await _dataContext.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderCode == orderCode)
                .ToListAsync();

            var coupon = string.IsNullOrEmpty(order.CouponCode)
                ? null
                : await _dataContext.Coupons.FirstOrDefaultAsync(c => c.CouponCode == order.CouponCode);

            ViewBag.Order = order;
            ViewBag.Coupon = coupon;
            return View(orderDetails);
        }

        [HttpGet]

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(UserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null)
            {
                TempData["error"] = "Email does not exist.";
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action("NewPassword", "Account", new
            {
                email = user.Email,
                token = HttpUtility.UrlEncode(token)
            }, Request.Scheme);

            var body = $"Ấn Vào <a href='{callbackUrl}'>Đây</a> Để Đặt Lại Mật Khẩu.";

            await _emailSender.SendEmailAsync(user.Email!, "Đặt Lại Mật Khẩu", body);

            TempData["success"] = "Password reset email sent.";
            return RedirectToAction("ForgotPassword");
        }


        [HttpGet]
        public IActionResult NewPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = HttpUtility.UrlDecode(token)
            });
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null)
            {
                TempData["error"] = "Email does not exist.";
                return RedirectToAction("ForgotPassword");
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                HttpUtility.UrlDecode(model.Token!),
                model.NewPassword!
            );

            if (result.Succeeded)
            {
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UserOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("No personal information found.");
            }

            var model = new UserViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                District = user.District,
                Ward = user.Ward
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UserOrder(UserViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.City = model.City;
            user.District = model.District;
            user.Ward = model.Ward;

            await _userManager.UpdateAsync(user);

            TempData["success"] = "Personal information has been updated successfully.";
            return RedirectToAction("UserOrder");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["error"] = "Please enter complete information.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["error"] = "Passwords do not match.";
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["success"] = "Password changed successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }
    }
}
