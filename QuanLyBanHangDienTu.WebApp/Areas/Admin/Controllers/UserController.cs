using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBanHangDienTu.WebApp.Models.ViewModels;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Repository.Validation;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;

namespace QuanLyBanHangDienTu.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UserController(DataContext dataContext, UserManager<UserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var usersWithRolesQuery = from u in _dataContext.Users
                                      join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                      join r in _dataContext.Roles on ur.RoleId equals r.Id
                                      select new UserWithRoleViewModel
                                      {
                                          User = u,
                                          RoleName = r.Name
                                      };

            int count = await usersWithRolesQuery.CountAsync();
            var pager = new Paginate(count, page, pageSize);

            var usersWithRoles = await usersWithRolesQuery
                .OrderByDescending(x => x.User!.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            return View(usersWithRoles);
        }


        [HttpGet]
        public async Task<IActionResult> Add()
        {
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Id", "Name");
            return View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(UserModel model, string selectedRoleId)
        {
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Id", "Name", selectedRoleId);

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid data.";
                return View(model);
            }

            var createResult = await _userManager.CreateAsync(model, model.PasswordHash!);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            var role = await _roleManager.FindByIdAsync(selectedRoleId);
            if (role != null)
            {
                var roleResult = await _userManager.AddToRoleAsync(model, role.Name!);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }

            TempData["success"] = "User added successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var currentRole = currentRoleName != null ? await _roleManager.FindByNameAsync(currentRoleName) : null;

            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Id", "Name", currentRole?.Id);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserModel model, string selectedRoleId)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Id", "Name", selectedRoleId);

            if (!ModelState.IsValid) return View(model);

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var role = await _roleManager.FindByIdAsync(selectedRoleId);
            if (role != null)
                await _userManager.AddToRoleAsync(user, role.Name!);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            TempData["success"] = "User updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {               
                TempData["success"] = "User deleted successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Role cannot be deleted.";
            }    
            
            return RedirectToAction("Index");
        }
    }
}
