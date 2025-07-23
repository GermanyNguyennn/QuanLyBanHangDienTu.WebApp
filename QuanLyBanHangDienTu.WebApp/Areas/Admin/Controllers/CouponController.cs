using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Repository;
using QuanLyBanHangDienTu.WebApp.Repository.Validation;

namespace QuanLyBanHangDienTu.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly DataContext _dataContext;

        public CouponController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            int count = await _dataContext.Coupons.CountAsync();
            var pager = new Paginate(count, page, pageSize);

            var coupons = await _dataContext.Coupons
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            return View(coupons);
        }


        [HttpGet]
        public IActionResult Add() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CouponModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid data.";
                return View(model);
            }

            if (model.StartDate >= model.EndDate)
            {
                TempData["error"] = "Start date must be less than end date.";
                return View(model);
            }

            bool isExist = await _dataContext.Coupons
                .AnyAsync(c => c.CouponCode!.ToLower() == model.CouponCode!.ToLower());

            if (isExist)
            {
                TempData["error"] = "Coupon code already exists.";
                return View(model);
            }

            _dataContext.Coupons.Add(model);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Coupon code added successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _dataContext.Coupons.FindAsync(id);
            if (coupon == null)
            {
                TempData["error"] = "Coupon code not found.";
                return RedirectToAction("Index");
            }

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid data.";
                return View(model);
            }

            if (model.StartDate >= model.EndDate)
            {
                TempData["error"] = "Start date must be less than end date.";
                return View(model);
            }

            var coupon = await _dataContext.Coupons.FindAsync(model.Id);
            if (coupon == null)
            {
                TempData["error"] = "Coupon code not found.";
                return RedirectToAction("Index");
            }

            bool isExist = await _dataContext.Coupons
                .AnyAsync(c => c.Id != model.Id && c.CouponCode!.ToLower() == model.CouponCode!.ToLower());

            if (isExist)
            {
                TempData["error"] = "The coupon code already matches another code.";
                return View(model);
            }

            coupon.CouponCode = model.CouponCode;
            coupon.Description = model.Description;
            coupon.DiscountType = model.DiscountType;
            coupon.DiscountValue = model.DiscountValue;
            coupon.StartDate = model.StartDate;
            coupon.EndDate = model.EndDate;
            coupon.Quantity = model.Quantity;
            coupon.Status = model.Status;

            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Coupon code updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _dataContext.Coupons.FindAsync(id);
            if (coupon == null)
            {
                TempData["error"] = "Coupon code not found.";
                return RedirectToAction("Index");
            }

            _dataContext.Coupons.Remove(coupon);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Coupon code deleted successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var coupon = await _dataContext.Coupons.FindAsync(id);
            if (coupon == null)
            {
                TempData["error"] = "Coupon code not found.";
                return RedirectToAction("Index");
            }

            coupon.Status = coupon.Status == 1 ? 0 : 1;
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Coupon code status has been updated.";
            return RedirectToAction("Index");
        }
    }
}
