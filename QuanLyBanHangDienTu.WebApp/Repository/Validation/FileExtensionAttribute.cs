using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHangDienTu.WebApp.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions = new[] { ".jpg", ".png", ".jpeg", ".webp" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!_allowedExtensions.Contains(extension))
                {
                    return new ValidationResult($"Only these file extensions are allowed: {string.Join(", ", _allowedExtensions)}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
