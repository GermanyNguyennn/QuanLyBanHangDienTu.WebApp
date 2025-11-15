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
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            int count = await _dataContext.Sliders.CountAsync();
            var pager = new Paginate(count, page, pageSize);

            var sliders = await _dataContext.Sliders
                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            return View(sliders);
        }

        [HttpGet]
        public IActionResult Add() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(SliderModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid data.";
                return View(model);
            }

            if (model.ImageUpload != null)
            {
                model.Image = await SaveImage(model.ImageUpload);
            }

            _dataContext.Sliders.Add(model);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Slider added successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _dataContext.Sliders.FindAsync(id);
            if (slider == null) return NotFound();

            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel model)
        {
            var existing = await _dataContext.Sliders.FindAsync(model.Id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid data.";
                return View(model);
            }

            if (model.ImageUpload != null)
            {
                await DeleteImage(existing.Image!);
                existing.Image = await SaveImage(model.ImageUpload);
            }

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Status = model.Status;

            _dataContext.Update(existing);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Slider updatec successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _dataContext.Sliders.FindAsync(id);
            if (slider == null) return NotFound();

            await DeleteImage(slider.Image!);

            _dataContext.Sliders.Remove(slider);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Slider deleted successfully!";
            return RedirectToAction("Index");
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            var folder = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
            var fileName = Guid.NewGuid() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(folder, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return fileName;
        }

        private async Task DeleteImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName) || imageName.Equals("null.jpg", StringComparison.OrdinalIgnoreCase))
                return;

            var path = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders", imageName);
            if (System.IO.File.Exists(path))
            {
                await Task.Run(() => System.IO.File.Delete(path));
            }
        }
    }
}
