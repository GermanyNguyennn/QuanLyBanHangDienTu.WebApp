using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHangDienTu.WebApp.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required, EmailAddress]
        public string? Email { get; set; }
        [Required, Phone]
        public string? PhoneNumber { get; set; }
    }
}
