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

        public async Task<IActionResult> Index()
        {
            await Set2FAStatusAsync();
            return View();
        }

        public async Task Set2FAStatusAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var is2FAEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                var isRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);

                ViewBag.Is2FACompleted = !is2FAEnabled || isRemembered;
                ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            }
            else
            {
                ViewBag.Is2FACompleted = false;
                ViewBag.IsAdmin = false;
            }
        }

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

        private async Task<string> EnsureAuthenticatorKeyAsync(UserModel user)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            return key!;
        }

        private string GenerateQrCodeUrl(string email, string key, string userName)
        {
            string issuer = $"{userName}";
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
                   $"?secret={key}&issuer={Uri.EscapeDataString(issuer)}";
        }

        [HttpGet]
        public async Task<IActionResult> Enable2FA()
        {
            var user = await GetUserFromContextAsync();
            if (user == null) return RedirectToAction("Login");

            if (await _userManager.GetTwoFactorEnabledAsync(user))
                return RedirectToAction("Index", "Home");

            var key = await EnsureAuthenticatorKeyAsync(user);
            ViewBag.QrCodeUrl = GenerateQrCodeUrl(user.Email!, key, user.UserName!);
            ViewBag.Key = key;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Enable2FA(string verificationCode)
        {
            var user = await GetUserFromContextAsync();
            if (user == null) return RedirectToAction("Login");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, verificationCode);
            if (!isValid)
            {
                var key = await EnsureAuthenticatorKeyAsync(user);
                ViewBag.QrCodeUrl = GenerateQrCodeUrl(user.Email!, key, user.UserName!);
                ViewBag.Key = key;
                TempData["error"] = "Invalid Verification Code.";
                return View();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            TempData["success"] = "2-Step Verification Enabled.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Verify2FA()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FA(string verificationCode)
        {
            if (string.IsNullOrWhiteSpace(verificationCode))
            {
                TempData["error"] = "Please enter verification code.";
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return RedirectToAction("Login");

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(verificationCode, false, false);

            if (result.Succeeded)
            {
                HttpContext.Session.SetString("Is2FACompleted", "true");
                HttpContext.Session.SetString("IsAdmin", (await _userManager.IsInRoleAsync(user, "Admin")) ? "true" : "false");
                TempData["success"] = "Log in successfully.";
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut) return RedirectToAction("Lockout", "Account");

            TempData["error"] = "Invalid Verification Code.";
            return View();
        }

        [HttpGet]
        public IActionResult Reset2FA()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset2FA(Reset2FAViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["error"] = "Email does not exist.";
                return View();
            }

            var token = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "Reset2FA");
            var resetLink = Url.Action("ConfirmReset2FA", "Account", new { userId = user.Id, token }, Request.Scheme);

            await _emailSender.SendEmailAsync(model.Email!, "Reset 2FA", $"Ấn Vào <a href='{resetLink}'>Đây</a> Để Đặt Lại Xác Thực 2 Bước.");
            TempData["success"] = "A 2FA reset link has been sent to your email.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmReset2FA(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "Reset2FA", token);
            if (!isValid)
            {
                TempData["error"] = "The link is invalid or expired.";
                return RedirectToAction("Login", "Account");
            }

            var disableResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disableResult.Succeeded)
            {
                TempData["error"] = "Unable to reset 2FA.";
                return RedirectToAction("Login", "Account");
            }

            TempData["success"] = "2FA reset successful. Please log in again to set up 2FA.";
            return RedirectToAction("Login", "Account");
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

            if (result.RequiresTwoFactor)
            {
                TempData["UserId"] = user.Id;
                return RedirectToAction("Verify2FA", new { returnUrl = model.ReturnURL });
            }

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin") &&
                    !await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    TempData["UserId"] = user.Id;
                    return RedirectToAction("Enable2FA", new { returnUrl = model.ReturnURL });
                }

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

        public async Task<IActionResult> History(int page = 1)
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


        [HttpPost]
        public async Task<IActionResult> SendMailForgotPassword(UserModel UserModel)
        {
            if (!ModelState.IsValid)
                return View("ForgotPassword");

            var user = await _userManager.FindByEmailAsync(UserModel.Email!);
            if (user == null)
            {
                TempData["error"] = "Email does not exist.";
                return RedirectToAction("ForgotPassword");
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


        public IActionResult ForgotPassword()
        {
            return View();
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
        public async Task<IActionResult> UpdateNewPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("NewPassword", model);

            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null)
            {
                TempData["error"] = "Email does not exist.";
                return RedirectToAction("ForgotPassword");
            }

            var result = await _userManager.ResetPasswordAsync(user, HttpUtility.UrlDecode(model.Token!), model.NewPassword!);

            if (result.Succeeded)
            {
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View("NewPassword", model);
        }

        [HttpGet]
        public async Task<IActionResult> UserInformation()
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

            var model = new UserInformationViewModel
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
        public async Task<IActionResult> UserInformation(UserInformationViewModel model)
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
            return RedirectToAction("UserInformation");
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
