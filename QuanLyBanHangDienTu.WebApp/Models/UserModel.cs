using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class UserModel : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
