using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHangDienTu.WebApp.Models.ViewModels
{
    public class Reset2FAViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
