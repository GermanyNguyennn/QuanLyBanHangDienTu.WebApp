using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHangDienTu.WebApp.Repository.Validation;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class SliderModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
