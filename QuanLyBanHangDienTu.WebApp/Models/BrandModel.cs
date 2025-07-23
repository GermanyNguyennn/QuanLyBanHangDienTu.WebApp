using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class BrandModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public ICollection<ProductModel> Product { get; set; } = new List<ProductModel>();
    }
}
