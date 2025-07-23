using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHangDienTu.WebApp.Repository.Validation;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class ContactModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoImage { get; set; }
        public string Description { get; set; }
        public string Map { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

    }
}
